using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null; // Singleton

    public string seed; // For repeatability of randomness

    public GameObject player;

    [HideInInspector]
    public System.Random prng;

    private BoardManager boardManager;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        if (string.IsNullOrEmpty(seed)) { // Just get a random one
            seed = DateTime.Now.Ticks.ToString();
        }
        prng = new System.Random(seed.GetHashCode());

        boardManager = GetComponent<BoardManager>();
        StartGame();
    }

    void StartGame() {
        boardManager.NewBoard();        
        // Get the room to spawn in
        GameObject spawnRoom = boardManager.RandomRoom();
        // Get the square in the room to spawn in
        Transform floor = spawnRoom.transform.FindChild("floor");
        int i = prng.Next(0, floor.childCount);
        Vector2 spawnPos = floor.GetChild(i).position;

        // Spawn the player
        Instantiate(player, spawnPos, Quaternion.identity);

        // initialize scores
    }
}
