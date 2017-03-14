using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    // Makes a room instantiates it in the game world. Returns a GameObject containing all of it.
    public static GameObject MakeRandomRoom(GameObject[] floorTiles, GameObject[] wallTiles, Vector2 start, int width, int height) {
        // Create walls
        GameObject topWall = new GameObject("top_wall");
        GameObject botWall = new GameObject("bot_wall");
        GameObject leftWall = new GameObject("left_wall");
        GameObject rightWall = new GameObject("right_wall");
        GameObject walls = new GameObject("walls");
        topWall.transform.parent = botWall.transform.parent = leftWall.transform.parent = rightWall.transform.parent = walls.transform;

        GameObject newWallTile;

        // first, the corners
        newWallTile = Instantiate(wallTiles[4], new Vector2(start.x, start.y+height), Quaternion.identity); // Top left
        newWallTile.transform.parent = topWall.transform;

        newWallTile = Instantiate(wallTiles[5], new Vector2(start.x+width, start.y+height), Quaternion.identity); // Top right
        newWallTile.transform.parent = topWall.transform;

        newWallTile = Instantiate(wallTiles[6], new Vector2(start.x+width, start.y), Quaternion.identity); // bot right
        newWallTile.transform.parent = botWall.transform;

        newWallTile = Instantiate(wallTiles[7], new Vector2(start.x, start.y), Quaternion.identity); // bot left
        newWallTile.transform.parent = botWall.transform;

        // Now, fill in the wall outline
        for (int x = (int)start.x + 1; x <= (int)start.x + width - 1; x++) {
            // Top wall
            newWallTile = Instantiate(wallTiles[0], new Vector2(x, start.y+height), Quaternion.identity);
            newWallTile.transform.parent = topWall.transform;

            // Bot wall
            newWallTile = Instantiate(wallTiles[2], new Vector2(x, start.y), Quaternion.identity);
            newWallTile.transform.parent = botWall.transform;
        }

        for (int y = (int)start.y + 1; y <= (int)start.y + height - 1; y++) {
            // Left wall
            newWallTile = Instantiate(wallTiles[1], new Vector2(start.x, y), Quaternion.identity);
            newWallTile.transform.parent = leftWall.transform;
            // Right wall
            newWallTile = Instantiate(wallTiles[3], new Vector2(start.x+width, y), Quaternion.identity);
            newWallTile.transform.parent = rightWall.transform;
        }

        // Fill out the floor
        GameObject floor = new GameObject("floor");
        for (int x = (int)start.x + 1; x <= (int)start.x + width - 1; x++) {
            for (int y = (int)start.y + 1; y <= (int)start.y + height - 1; y++) {
                GameObject newFloorTile = Instantiate(floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(x, y), Quaternion.identity);
                newFloorTile.transform.parent = floor.transform;
            }
        }

        // Finalize
        GameObject room = new GameObject("Room_" + width + "x" + height); // ex: Room_32x32
        walls.transform.parent = floor.transform.parent = room.transform;
        return room;
    }
}
