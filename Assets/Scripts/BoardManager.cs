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

        // TODO the door creation logic occasionally creates islands. 
        // Needs to be refactored to ensure every room can get to every other room.
        List<Room.Door> doors = new List<Room.Door>();
        List<Room.Door> legalDoorDirections = GetLegalDoorDirections(w, h);

        foreach (Room.Door d in legalDoorDirections) {
            if (GameManager.instance.prng.Next(0, 100) < doorSpawnChance) {
                doors.Add(d);
            }
        }
        // If no doors were instantiated, create one in a legal position.
        // If no legal positions, just give up
        if (doors.Count == 0 && legalDoorDirections.Count > 0) {
            doors.Add(legalDoorDirections[GameManager.instance.prng.Next(0, legalDoorDirections.Count)]);
        }

        newRoom.Setup(floorTiles, wallTiles, botLeft, width, height, doors, isOuterRoom);
        return newRoom;
    }

    private List<Room.Door> GetLegalDoorDirections(int w, int h) {
        List<Room.Door> legalDoorDirections = new List<Room.Door>();
        foreach (Room.Door d in Enum.GetValues(typeof(Room.Door))) {
            // Don't spawn doors on the edge of the game
            if ((w == 0 && d == Room.Door.Left) ||
                (w == gameSize - 1 && d == Room.Door.Right) ||
                (h == 0 && d == Room.Door.Bot) ||
                (h == gameSize - 1 && d == Room.Door.Top)) {
                continue;
            } else {  // legal
                legalDoorDirections.Add(d);
            }
        }
        return legalDoorDirections;
    }
}
