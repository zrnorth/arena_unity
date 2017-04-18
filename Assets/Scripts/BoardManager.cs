using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    public GameObject[] floorTiles; // Floor prefabs
    [Tooltip("TOP, LEFT, BOT, RIGHT, TOPLEFT, TOPRIGHT, BOTRIGHT, BOTLEFT")]
    public GameObject[] wallTiles; // Wall prefabs. Order matters!
    
    public int height = 16; // Standard room height
    public int width = 16; // Standard room width

    public int gameSize = 3; // Game board will be gameSize x gameSize rooms.
    [Range(0, 100)]
    public int doorSpawnChance = 100;

    private Room[,] rooms;
    private Room centerRoom;
    private Pair centerRoomIndex;
    private Transform boardHolder;

    public void NewBoard() {
        // Clear the old board if it exists
        if (boardHolder != null) {
            Destroy(boardHolder.gameObject);
        }
        // Create game object to hold the board
        boardHolder = new GameObject("Board").transform;

        // Instantiate the rooms array
        rooms = new Room[gameSize, gameSize];

        // create rooms
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                Room newRoom = CreateNewRoom(w, h);

                newRoom.gameObject.transform.parent = boardHolder;
                rooms[w, h] = newRoom;
            }
        }

        // Set one room as the "center" or root room. Can't be a room on the outer edge.
        centerRoomIndex = new Pair(GameManager.instance.prng.Next(1, gameSize - 1), 
                                   GameManager.instance.prng.Next(1, gameSize - 1));
        centerRoom = GetRoomFromIndex(centerRoomIndex);

        // The center room should have doors on all four sides
        AddDoorsToRoom(centerRoomIndex, new HashSet<Room.Door> { Room.Door.Top, Room.Door.Left, Room.Door.Bot, Room.Door.Right });
        Debug.Log("Center room: " + centerRoomIndex);

        // Do a pass over the board, adding rooms by % chance
        PopulateBoardWithRandomDoors();

        // Do a spread fill to see which rooms are reachable from the center room. If some aren't reachable, keep adding doors til they are.
        List<Pair> reachableRoomsFromCenter = GetAllReachableRoomIndices(centerRoomIndex);
        int numRooms = gameSize * gameSize;
        int x = 0;
        while (reachableRoomsFromCenter.Count < numRooms && x < 100) {
            Debug.Log("Pass " + x);
            x++; // debug so we can't loop forever

            // Pick a random room, and add a door to it, then re-calculate
            bool succeeded = AddRandomDoors(reachableRoomsFromCenter, 1);
            // If we failed to add a room, something went wrong. Just break
            if (!succeeded) {
                break;
            }
            reachableRoomsFromCenter = GetAllReachableRoomIndices(centerRoomIndex);
        }
    }

    // Randomly adds a room at each possible intersection based on the specified % chance.
    void PopulateBoardWithRandomDoors() {
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                HashSet<Room.Door> doorsToAdd = new HashSet<Room.Door>();
                HashSet<Room.Door> possibleDoorsForRoom = GetPossibleNewDoors(w, h, true);
                foreach (Room.Door d in possibleDoorsForRoom) {
                    if (GameManager.instance.prng.Next(0, 100) < doorSpawnChance) {
                        doorsToAdd.Add(d);
                    }
                }
                AddDoorsToRoom(new Pair(w, h), doorsToAdd);
            }
        }
    }

    // Returns a list of all reachable rooms from a given room index.
    List<Pair> GetAllReachableRoomIndices(Pair start) {
        List<Pair> visited = new List<Pair>();
        Queue<Pair> q = new Queue<Pair>();
        q.Enqueue(start);

        int x = 0;
        while (q.Count > 0 && x < 100) {
            x++;
            Pair coords = q.Dequeue();

            if (visited.Contains(coords)) {
                continue;
            }

            visited.Add(coords);

            Room queuedRoom = GetRoomFromIndex(coords);
            queuedRoom.TintFloor(Color.green); // debug, Tint as you go

            foreach (Room.Door door in queuedRoom.doors) {
                Pair coordsToEnqueue = new Pair(-1, -1);

                switch (door) {
                    case Room.Door.Top:
                        coordsToEnqueue = new Pair(coords.w, coords.h + 1);
                        break;
                    case Room.Door.Bot:
                        coordsToEnqueue = new Pair(coords.w, coords.h - 1);
                        break;
                    case Room.Door.Left:
                        coordsToEnqueue = new Pair(coords.w - 1, coords.h);
                        break;
                    case Room.Door.Right:
                        coordsToEnqueue = new Pair(coords.w + 1, coords.h);
                        break;
                }
                if (coordsToEnqueue.w == -1) { // no doors
                    continue;
                }
                if (!visited.Contains(coordsToEnqueue)) {
                    q.Enqueue(coordsToEnqueue);
                }
            }
        }

        return visited;
    }

    // Adds random doors to random rooms in the list of room indices.
    // Returns false if none possible.
    bool AddRandomDoors(List<Pair> roomIndices, int numDoorsToAdd) {
        for (int i = 0; i < numDoorsToAdd; i++) {
            if (roomIndices.Count == 0) {
                return false;
            }
            // Get the room to add the random door to.
            Pair roomIndex = roomIndices[GameManager.instance.prng.Next(0, roomIndices.Count)];
            // Get the random door from the list of possible doors to add. If none possible, remove it from the
            // list of room indices and try again
            HashSet<Room.Door> possibleNewDoors = GetPossibleNewDoors(roomIndex.w, roomIndex.h);
            if (possibleNewDoors.Count == 0) {
                roomIndices.Remove(roomIndex);
                i--;
                continue;
            }
            Room.Door doorToAdd = GetRandom<Room.Door>(possibleNewDoors);

            bool ret = AddDoorToRoom(roomIndex, doorToAdd);
            if (!ret) { // something went wrong
                Debug.Log("Couldn't add a random door.");
                return false;
            }
        }
        return true;
    }

    // Helper - gets a single random element from a HashSet.
    static T GetRandom<T>(HashSet<T> hs) {
        T result = default(T);
        int x = GameManager.instance.prng.Next(0, hs.Count);
        int i = 0;
        foreach(T t in hs) {
            if (i == x) {
                result = t;
                break;
            }
            i++;
        }
        return result;
    }

    // Get a random room in the game.
    public Room RandomRoom() {
        int x = GameManager.instance.prng.Next(0, gameSize);
        int y = GameManager.instance.prng.Next(0, gameSize);
        return rooms[x,y];
    }

    // Get a inner room index.
    Pair GetRandomInnerRoomIndex() {
        return new Pair(GameManager.instance.prng.Next(1, gameSize - 1),
                        GameManager.instance.prng.Next(1, gameSize - 1));
    }

    // Get an edge (outer) room index.
    Pair GetRandomOuterRoomIndex() {
        throw new NotImplementedException();
    }
    
    // Creates a new GameObject with the Room script attached, and runs the Setup script in Room.
    Room CreateNewRoom(int w, int h) {
        Room newRoom = new GameObject().AddComponent<Room>();
        Vector2 botLeft = new Vector2((width+1) * w, (height+1) * h);
        bool isOuterRoom = (w == 0 || w == gameSize - 1 || h == 0 || h == gameSize - 1);

        newRoom.Setup(floorTiles, wallTiles, botLeft, width, height, isOuterRoom, new Pair(w, h));
        return newRoom;
    }

    // Returns the possible doors that could be added to a room.
    // We have the onlyTopAndRight flag for when we are making a full pass of all rooms and generating random doors.
    // We don't want to double-calculate when adding random rooms; locking ourselves to only checking
    // the top and right door (if possible) ensures each potential door only gets one possible chance.
    HashSet<Room.Door> GetPossibleNewDoors(int w, int h, bool onlyTopAndRight=false) {
        HashSet<Room.Door> existing = rooms[w, h].doors;
        HashSet<Room.Door> possibles = new HashSet<Room.Door>();
        if (w < gameSize - 1 && !existing.Contains(Room.Door.Right)) {
            possibles.Add(Room.Door.Right);
        }
        if (h < gameSize - 1 && !existing.Contains(Room.Door.Top)) {
            possibles.Add(Room.Door.Top);
        }
        if (w > 0 && !onlyTopAndRight && !existing.Contains(Room.Door.Left)) {
            possibles.Add(Room.Door.Left);
        }
        if (h > 0 && !onlyTopAndRight && !existing.Contains(Room.Door.Bot)) {
            possibles.Add(Room.Door.Bot);
        }

        return possibles;
    }

    Room GetRoomFromIndex(Pair index) {
        return rooms[index.w, index.h];
    }

    // Handles adding a door on both sides (the inputted room, and the connected room)
    bool AddDoorToRoom(Pair roomIndex, Room.Door doorToAdd) {
        Room baseRoom = GetRoomFromIndex(roomIndex);
        Pair otherRoomIndex = null;
        Room.Door otherDoorToAdd = Room.Door.Bot; // temp, needs a default value because enum
        switch(doorToAdd) {
            case Room.Door.Bot:
                otherRoomIndex = new Pair(roomIndex.w, roomIndex.h - 1);
                otherDoorToAdd = Room.Door.Top;
                break;
            case Room.Door.Top:
                otherRoomIndex = new Pair(roomIndex.w, roomIndex.h + 1);
                otherDoorToAdd = Room.Door.Bot;
                break;
            case Room.Door.Left:
                otherRoomIndex = new Pair(roomIndex.w - 1, roomIndex.h);
                otherDoorToAdd = Room.Door.Right;
                break;
            case Room.Door.Right:
                otherRoomIndex = new Pair(roomIndex.w + 1, roomIndex.h);
                otherDoorToAdd = Room.Door.Left;
                break;
        }
        if (otherRoomIndex == null) {
            return false;
        }

        Room otherRoom = GetRoomFromIndex(otherRoomIndex);
        baseRoom.AddDoor(doorToAdd);
        otherRoom.AddDoor(otherDoorToAdd);
        Debug.Log("Added door from " + roomIndex.ToString() + " to " + otherRoomIndex.ToString());
        return true;
    }

    // helper to add a multiple doors at once
    bool AddDoorsToRoom(Pair roomIndex, HashSet<Room.Door> doorsToAdd) {
        foreach (Room.Door doorToAdd in doorsToAdd) {
            bool succeeded = AddDoorToRoom(roomIndex, doorToAdd);
            if (!succeeded) {
                return false;
            }
        }
        return true;
    }
}
