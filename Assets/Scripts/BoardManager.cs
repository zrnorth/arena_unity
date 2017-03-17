using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    public GameObject[] floorTiles;
    [Tooltip("TOP, LEFT, BOT, RIGHT, TOPLEFT, TOPRIGHT, BOTRIGHT, BOTLEFT")]
    public GameObject[] wallTiles;

    public GameObject blankRoom;
    
    public int height = 16;
    public int width = 16;

    public int gameSize = 3; // Game board will be gameSize x gameSize rooms.

    private GameObject[,] rooms;
    private Transform boardHolder;

    public void NewBoard() {
        // Clear the old board if it exists
        if (boardHolder != null) {
            Destroy(boardHolder.gameObject);
        }
        // Create game object to hold the board
        boardHolder = new GameObject("Board").transform;

        // Instantiate the rooms array
        rooms = new GameObject[gameSize, gameSize];

        // create rooms
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                GameObject newRoom = Instantiate(blankRoom);
                newRoom.GetComponent<Room>().Setup(floorTiles, wallTiles, new Vector2(width*w, height* h), width, height);
                newRoom.gameObject.transform.parent = boardHolder;
                rooms[w, h] = newRoom;
            }
        }
    }

    // Helper function. Returns a random room in the game.
    public GameObject RandomRoom() {
        int x = GameManager.instance.prng.Next(0, gameSize);
        int y = GameManager.instance.prng.Next(0, gameSize);
        return rooms[x,y];
    }
}
