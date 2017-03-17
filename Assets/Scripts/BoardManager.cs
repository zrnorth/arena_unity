using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    public GameObject[] floorTiles;
    public GameObject[] wallTiles;

    public int height = 16;
    public int width = 16;

    public int gameSize = 3; // Game board will be gameSize x gameSize rooms.

    private GameObject[,] rooms;
    private Transform boardHolder;

    void Awake() {
        boardHolder = new GameObject("Board").transform;

        rooms = new GameObject[gameSize, gameSize];
        // create rooms
        for (int w = 0; w < gameSize; w++) {
            for (int h = 0; h < gameSize; h++) {
                GameObject r = Room.MakeRandomRoom(floorTiles, wallTiles, new Vector2(width*w, height*h), width, height);
                r.transform.parent = boardHolder;
                rooms[w, h] = r;
            }
        }
        // spawn player
        //SpawnPlayer();
        // initialize scores
    }
}
