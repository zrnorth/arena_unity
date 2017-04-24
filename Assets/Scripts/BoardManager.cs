using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    public GameObject[] floorTiles; // Floor prefabs
    [Tooltip("TOP, LEFT, BOT, RIGHT, TOPLEFT, TOPRIGHT, BOTRIGHT, BOTLEFT")]
    public GameObject[] wallTiles; // Wall prefabs. Order matters!
    public GameObject[] doorTiles; // Door prefabs
    
    public int height = 16; // Standard room height
    public int width = 16; // Standard room width

    public int gameSize = 3; // Game board will be gameSize x gameSize rooms.
    [Range(0, 100)]
    public int randomDoorSpawnChance = 100; // Note that some doors will always exist, because all rooms must be reachable.

    private Room[,] rooms;
    private Room centerRoom;
    private Pair centerRoomCoords;
    private Transform boardHolder;

    //debug
    private int numDoors = 0;

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
        centerRoomCoords = new Pair(GameManager.instance.prng.Next(1, gameSize - 1), 
                                   GameManager.instance.prng.Next(1, gameSize - 1));
        centerRoom = GetRoomFromCoords(centerRoomCoords);
        centerRoom.TintFloor(Color.black); // debug

        // The center room should have doors on all four sides
        AddDoorsToRoom(centerRoomCoords, new HashSet<Room.Door> { Room.Door.Top, Room.Door.Left, Room.Door.Bot, Room.Door.Right });
        Debug.Log("Center room: " + centerRoomCoords);

        // Do a pass over the board, adding rooms by % chance
        PopulateBoardWithRandomDoors();
        // Connect the remaining unconnected rooms to the center region.
        ConnectAllDoorsToCenter();

        // Finished!
        Summarize();
    }

    // Get a random room in the game.
    public Room RandomRoom() {
        int x = GameManager.instance.prng.Next(0, gameSize);
        int y = GameManager.instance.prng.Next(0, gameSize);
        return rooms[x, y];
    }



    // Randomly adds a room at each possible intersection based on the specified % chance.
    void PopulateBoardWithRandomDoors() {
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                HashSet<Room.Door> doorsToAdd = new HashSet<Room.Door>();
                HashSet<Room.Door> possibleDoorsForRoom = GetPossibleNewDoors(w, h, true);
                foreach (Room.Door d in possibleDoorsForRoom) {
                    if (GameManager.instance.prng.Next(0, 100) < randomDoorSpawnChance) {
                        doorsToAdd.Add(d);
                    }
                }
                AddDoorsToRoom(new Pair(w, h), doorsToAdd);
            }
        }
    }

    // Ensures that all the rooms have a path to the center.
    void ConnectAllDoorsToCenter() {
        List<Pair> reachableRoomsFromCenter = GetAllReachableRoomCoords(centerRoomCoords);
        List<Pair> frontierRooms = new List<Pair>();
        List<Pair> outskirtRooms = new List<Pair>();
        int numRooms = gameSize * gameSize;
        int passes = 0;

        while (reachableRoomsFromCenter.Count < numRooms && passes < 100) {
            Debug.Log("Pass " + passes);
            passes++; // debug so we can't loop forever

            PopulateFrontierAndOutskirtRooms(reachableRoomsFromCenter, out frontierRooms, out outskirtRooms);

            // Connect all the outskirt doors to the frontier. Break on failure.
            if (!ConnectOutskirtsToFrontier(outskirtRooms, frontierRooms)) {
                break;
            }
            // Regenerate the frontiers for the next pass
            reachableRoomsFromCenter = GetAllReachableRoomCoords(centerRoomCoords);
        }
    }

    // Given a list of connected rooms, populates the subset of those rooms that are adjacent to unconnected rooms.
    void PopulateFrontierAndOutskirtRooms(List<Pair> reachableRoomsFromCenter, out List<Pair> frontierRooms, out List<Pair> outskirtRooms) {
        // First, get a list of all the unconnected rooms
        List<Pair> unconnectedRooms = new List<Pair>();
        foreach (Pair p in AllRoomCoords()) {
            if (!reachableRoomsFromCenter.Contains(p)) {
                GetRoomFromCoords(p).TintFloor(Color.red); // debug
                unconnectedRooms.Add(p);
            }
        }

        // Use a hash set for the comparisons for easy uniqueness
        HashSet<Pair> uniqueFrontierRooms = new HashSet<Pair>();
        HashSet<Pair> uniqueOutskirtRooms = new HashSet<Pair>();

        // Now, compare each connected room to each unconnected room. If the connected room is one square from the unconnected, mark it as frontier / outskirt.
        foreach (Pair unconnectedRoom in unconnectedRooms) {
            foreach (Pair connectedRoom in reachableRoomsFromCenter) {
                if (RoomsAreAdjacent(unconnectedRoom, connectedRoom)) {
                    uniqueOutskirtRooms.Add(unconnectedRoom);
                    uniqueFrontierRooms.Add(connectedRoom);
                }
            }
        }

        // Convert to List for return format
        frontierRooms = new List<Pair>();
        outskirtRooms = new List<Pair>();
        foreach (Pair p in uniqueFrontierRooms) {
            frontierRooms.Add(p);
        }
        foreach (Pair p in uniqueOutskirtRooms) {
            outskirtRooms.Add(p);
        }
        // Debug
        if (frontierRooms.Count > 0) {
            Debug.Log(frontierRooms.Count + " frontier rooms:");
            string s = "[ ";
            foreach (Pair r in frontierRooms) {
                s += r.ToString() + ", ";
            }
            s = s.Substring(0, s.Length - 2);
            s += " ]";
            Debug.Log(s);
        }
        if (outskirtRooms.Count > 0) {
            Debug.Log(outskirtRooms.Count + " outskirt rooms:");
            string s = "[ ";
            foreach (Pair r in outskirtRooms) {
                s += r.ToString() + ", ";
            }
            s = s.Substring(0, s.Length - 2);
            s += " ]";
            Debug.Log(s);
        }
    }

    // Connects all the rooms in the outskirtRooms with a room in the frontierRooms.
    // Returns false if not possible, or if some failure.
    bool ConnectOutskirtsToFrontier(List<Pair> outskirtRooms, List<Pair> frontierRooms) {
        foreach (Pair outskirtRoom in outskirtRooms) {
            bool connected = false;
            foreach (Pair frontierRoom in frontierRooms) {
                if (RoomsAreAdjacent(outskirtRoom, frontierRoom)) {
                    connected = AddDoorToRoom(outskirtRoom, GetDoorBetweenRooms(outskirtRoom, frontierRoom));
                    break;
                }
            }
            if (!connected) {
                return false;
            }
        }
        return true;
    }

    // Logs a summary of the generated board. Helper for NewBoard()
    void Summarize() {
        string s = "Generated board! Statistics:";
        s += "\n*  Number of doors: " + numDoors;

        int maxNumberPossibleDoors = 2 * gameSize * (gameSize - 1);
        s += "\n*  Max number possible doors: " + maxNumberPossibleDoors;
        s += "\n*  Door density:" + (float)numDoors / (float)maxNumberPossibleDoors;

        Debug.Log(s);
    }

    // Returns a list of all reachable rooms from a given room coords.
    List<Pair> GetAllReachableRoomCoords(Pair start) {
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

            Room queuedRoom = GetRoomFromCoords(coords);
            if (coords != centerRoomCoords) {
                queuedRoom.TintFloor(Color.green); // debug, Tint as you go
            }

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

    Room GetRoomFromCoords(Pair coords) {
        return rooms[coords.w, coords.h];
    }

    // Helper function. Returns a list of all possible room coordinates.
    List<Pair> AllRoomCoords() {
        List<Pair> allRooms = new List<Pair>();
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                allRooms.Add(new Pair(w, h));
            }
        }
        return allRooms;
    }


    // Rooms are adjacent if their coords have either the w / h element exactly 1 apart from each other,
    // and the other the same.
    bool RoomsAreAdjacent(Pair roomA, Pair roomB) {
        if (roomA.w == roomB.w) {
            return (Mathf.Abs(roomA.h - roomB.h) == 1);
        } else if (roomA.h == roomB.h) {
            return (Mathf.Abs(roomA.w - roomB.w) == 1);
        } else return false;
    }

    // Returns the direction a door should be to connect roomA to roomB.
    // Throws an exception if the doors are not adjacent to each other.
    Room.Door GetDoorBetweenRooms(Pair from, Pair to) {
        if (from.w == to.w) {
            if (from.h == to.h - 1) {
                return Room.Door.Top;
            } else if (from.h == to.h + 1) {
                return Room.Door.Bot;
            }
        } else if (from.h == to.h) {
            if (from.w == to.w - 1) {
                return Room.Door.Right;
            } else if (from.w == to.w + 1) {
                return Room.Door.Left;
            }
        } 
        throw new InvalidOperationException("Can't make a door between two non-adjacent rooms");
    }

    // Adds random doors to random rooms in the list of room coords.
    // Returns false if none possible.
    bool AddRandomDoors(List<Pair> roomCoords, int numDoorsToAdd) {
        for (int i = 0; i < numDoorsToAdd; i++) {
            if (roomCoords.Count == 0) {
                return false;
            }
            // Get the room to add the random door to.
            Pair room = roomCoords[GameManager.instance.prng.Next(0, roomCoords.Count)];
            // Get the random door from the list of possible doors to add. If none possible, remove it from the
            // list of room coords and try again
            HashSet<Room.Door> possibleNewDoors = GetPossibleNewDoors(room.w, room.h);
            if (possibleNewDoors.Count == 0) {
                roomCoords.Remove(room);
                i--;
                continue;
            }
            Room.Door doorToAdd = GameManager.GetRandom<Room.Door>(possibleNewDoors);

            bool ret = AddDoorToRoom(room, doorToAdd);
            if (!ret) { // something went wrong
                Debug.Log("Couldn't add a random door.");
                return false;
            }
        }
        return true;
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


    // Handles adding a door on both sides (the inputted room, and the connected room)
    bool AddDoorToRoom(Pair roomCoords, Room.Door doorToAdd) {
        Room baseRoom = GetRoomFromCoords(roomCoords);
        Pair otherRoomCoords = null;
        Room.Door otherDoorToAdd = Room.Door.NULL;
        switch(doorToAdd) {
            case Room.Door.Bot:
                otherRoomCoords = new Pair(roomCoords.w, roomCoords.h - 1);
                otherDoorToAdd = Room.Door.Top;
                break;
            case Room.Door.Top:
                otherRoomCoords = new Pair(roomCoords.w, roomCoords.h + 1);
                otherDoorToAdd = Room.Door.Bot;
                break;
            case Room.Door.Left:
                otherRoomCoords = new Pair(roomCoords.w - 1, roomCoords.h);
                otherDoorToAdd = Room.Door.Right;
                break;
            case Room.Door.Right:
                otherRoomCoords = new Pair(roomCoords.w + 1, roomCoords.h);
                otherDoorToAdd = Room.Door.Left;
                break;
        }
        if (otherRoomCoords == null) {
            return false;
        }

        Room otherRoom = GetRoomFromCoords(otherRoomCoords);
        baseRoom.AddDoor(doorToAdd);
        otherRoom.AddDoor(otherDoorToAdd);
        Debug.Log("Added door from " + roomCoords.ToString() + " to " + otherRoomCoords.ToString());
        numDoors++;
        return true;
    }

    // helper to add a multiple doors at once
    bool AddDoorsToRoom(Pair roomCoords, HashSet<Room.Door> doorsToAdd) {
        foreach (Room.Door doorToAdd in doorsToAdd) {
            bool succeeded = AddDoorToRoom(roomCoords, doorToAdd);
            if (!succeeded) {
                return false;
            }
        }
        return true;
    }
}
