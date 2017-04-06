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

        // create doors
        // First pass: Add TOP and RIGHT
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                List<Room.Door> doorsToAdd = new List<Room.Door>();
                List<Room.Door> possibleDoorsForRoom = GetPossibleNewDoors(w, h);
                // Each door has a random chance of appearing
                foreach (Room.Door d in possibleDoorsForRoom) {
                    if (GameManager.instance.prng.Next(0, 100) < doorSpawnChance) {
                        doorsToAdd.Add(d);
                    }
                }
                rooms[w, h].AddDoors(doorsToAdd);
            }
        }
        // Second pass: Copy over BOT and LEFT based on the results of first pass.
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                List<Room.Door> doorsToAdd = DoorsFromBotAndLeftRooms(w, h);
                rooms[w, h].AddDoors(doorsToAdd);
            }
        }
    }

    // Helper function. Returns a random room in the game.
    public Room RandomRoom() {
        int x = GameManager.instance.prng.Next(0, gameSize);
        int y = GameManager.instance.prng.Next(0, gameSize);
        return rooms[x,y];
    }

    // Creates a new GameObject with the Room script attached, and runs the Setup script in Room.
    Room CreateNewRoom(int w, int h) {
        Room newRoom = new GameObject().AddComponent<Room>();
        Vector2 botLeft = new Vector2((width+1) * w, (height+1) * h);
        bool isOuterRoom = (w == 0 || w == gameSize - 1 || h == 0 || h == gameSize - 1);

        newRoom.Setup(floorTiles, wallTiles, botLeft, width, height, isOuterRoom);
        return newRoom;
    }

    // First pass 
    // Returns the possible doors that could be added to a room, assuming that rooms care about UP and RIGHT doors.
    // On the right-most column, they can't have a RIGHT door.
    // On the top-most row, they can't have an UP door.
    List<Room.Door> GetPossibleNewDoors(int w, int h) {
        List<Room.Door> possibles = new List<Room.Door>();
        if (w < gameSize - 1) {
            possibles.Add(Room.Door.Right);
        }
        if (h < gameSize - 1) {
            possibles.Add(Room.Door.Top);
        }
        return possibles;
    }

    // This returns a list of doors that we need for the second "pass."
    // Assuming we have filled in the room array with TOP / RIGHT doors randomly, 
    // we copy over those door values to the other rooms so they all match up with
    // corresponding BOT / LEFT doors.
    List<Room.Door> DoorsFromBotAndLeftRooms(int w, int h) {
        List<Room.Door> doors = new List<Room.Door>();
        if (w > 0 && rooms[w-1, h].doors.Contains(Room.Door.Right)) {
            doors.Add(Room.Door.Left);
        }
        if (h > 0 && rooms[w, h - 1].doors.Contains(Room.Door.Top)) {
            doors.Add(Room.Door.Bot);
        }
        return doors;
    }
}
