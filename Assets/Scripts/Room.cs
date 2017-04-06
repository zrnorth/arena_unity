using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public enum Door {
        Top,
        Left,
        Bot,
        Right
    };

    public int width { get; set; }
    public int height { get; set; }
    public List<Door> doors = new List<Door>();
    public bool isOuterRoom; // true if this room is on the outer rim of the grid


    // Actually sets up a room with its vars & instantiates it in the game world.
    // At first, rooms are made without doors. Uses AddDoor() to add them after this is run.
    public void Setup(GameObject[] floorTiles, GameObject[] wallTiles, Vector2 botLeft, int _width, int _height, bool _isOuterRoom) {
        width = _width;
        height = _height;
        isOuterRoom = _isOuterRoom;

        // Position us correctly in the scene
        gameObject.transform.position = new Vector2((botLeft.x + width / 2), (botLeft.y + height / 2));

        // Create walls
        GameObject topWall = new GameObject("top_wall");
        GameObject botWall = new GameObject("bot_wall");
        GameObject leftWall = new GameObject("left_wall");
        GameObject rightWall = new GameObject("right_wall");
        GameObject walls = new GameObject("walls");
        topWall.transform.parent = botWall.transform.parent = leftWall.transform.parent = rightWall.transform.parent = walls.transform;

        GameObject newWallTile;

        // first, the corners
        newWallTile = Instantiate(wallTiles[4], new Vector2(botLeft.x, botLeft.y + height), Quaternion.identity); // Top left
        newWallTile.transform.parent = topWall.transform;

        newWallTile = Instantiate(wallTiles[5], new Vector2(botLeft.x + width, botLeft.y + height), Quaternion.identity); // Top right
        newWallTile.transform.parent = topWall.transform;

        newWallTile = Instantiate(wallTiles[6], new Vector2(botLeft.x + width, botLeft.y), Quaternion.identity); // bot right
        newWallTile.transform.parent = botWall.transform;

        newWallTile = Instantiate(wallTiles[7], new Vector2(botLeft.x, botLeft.y), Quaternion.identity); // bot left
        newWallTile.transform.parent = botWall.transform;

        // Now, fill in the wall outline
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            // Top wall
            newWallTile = Instantiate(wallTiles[0], new Vector2(x, botLeft.y + height), Quaternion.identity);
            newWallTile.transform.parent = topWall.transform;

            // Bot wall
            newWallTile = Instantiate(wallTiles[2], new Vector2(x, botLeft.y), Quaternion.identity);
            newWallTile.transform.parent = botWall.transform;
        }

        for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
            // Left wall
            newWallTile = Instantiate(wallTiles[1], new Vector2(botLeft.x, y), Quaternion.identity);
            newWallTile.transform.parent = leftWall.transform;
            // Right wall
            newWallTile = Instantiate(wallTiles[3], new Vector2(botLeft.x + width, y), Quaternion.identity);
            newWallTile.transform.parent = rightWall.transform;
        }

        // Fill out the floor
        GameObject floor = new GameObject("floor");
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
                GameObject newFloorTile = Instantiate(floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(x, y), Quaternion.identity);
                newFloorTile.transform.parent = floor.transform;
            }
        }

        // Finalize
        walls.transform.parent = floor.transform.parent = gameObject.transform;
        UpdateRoomName();
    }

    public void AddDoors(List<Door> doorsToAdd) {
        doors = doorsToAdd;
        TintDoors();
        UpdateRoomName();
    }

    // DEBUG tints the doors blue
    private void TintDoors() {
        Transform walls = gameObject.transform.Find("walls");
        foreach (Door d in doors) {
            string sideName = d.ToString().ToLower() + "_wall";
            Transform side = walls.transform.Find(sideName);
            for (int i = 8; i < 11; i++) {
                side.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
    }

    // Changes the room game object's name to reflect its current state
    private void UpdateRoomName() {
        string roomName = "";
        if (isOuterRoom) {
            roomName += "Outer_";
        } else {
            roomName += "Inner_";
        }
        roomName += "Room_" + width + "x" + height;
        if (doors.Count > 0) {
            roomName += " | Doors:";
            foreach (Door d in doors) {
                roomName += " " + d.ToString();
            }
        }

        gameObject.name = roomName;
    }
}
