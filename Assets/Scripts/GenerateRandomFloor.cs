using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRandomFloor : MonoBehaviour {
    public GameObject[] floorPrefabs;
    public int width = 14; // must be even!
    public int height = 14;
    public int centerX = 0;
    public int centerY = 0;

	void Awake () {
        int widthDivTwo = width / 2;
        int heightDivTwo = height / 2;
		for (int w = -widthDivTwo; w <= widthDivTwo; w++) {
            for (int h = -heightDivTwo; h <= heightDivTwo; h++) {
                GameObject newFloorTile = Instantiate(floorPrefabs[Random.Range(0, floorPrefabs.Length)], new Vector2(w, h), Quaternion.identity);
                newFloorTile.transform.parent = gameObject.transform;
            }
        }
	}
	
}
