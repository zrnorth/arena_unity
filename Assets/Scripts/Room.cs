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
    public HashSet<Door> doors = new HashSet<Door>();
    public bool isOuterRoom; // true if this room is on the outer rim of the grid
    public Pair boardCoords { get; set; } // Position on the board in terms of rooms


    // Actually sets up a room with its vars & instantiates it in the game world.
    // At first, rooms are made without doors. Uses AddDoor() to add them after this is run.
    public void Setup(GameObject[] floorTiles, GameObject[] wallTiles, Vector2 botLeft, int _width, int _height, bool _isOuterRoom, Pair _boardCoords) {
        width = _width;
        height = _height;
        isOuterRoom = _isOuterRoom;
        boardCoords = _boardCoords;

        // Position us correctly in the scene
        gameObject.transform.position = new Vector2((botLeft.x + width / 2), (botLeft.y + height / 2));

        // Create walls
        GameObject topWall = new GameObject("top_wall");
        GameObject botWall = new GameObject("bot_wall");
        GameObject leftWall = new GameObject("left_wall");
        GameObject rightWall = new GameObject("right_wall");
        GameObject walls = new GameObject("walls");
        topWall.transform.parent = botWall.transform.parent = leftWall.transform.parent = rightWall.transform.parent = walls.transform;

        // first, the corners
        CreateTile(wallTiles[4], new Vector2(botLeft.x, botLeft.y + height), topWall.transform); // Top left
        CreateTile(wallTiles[5], new Vector2(botLeft.x + width, botLeft.y + height), topWall.transform); // Top right
        CreateTile(wallTiles[6], new Vector2(botLeft.x + width, botLeft.y), botWall.transform); // bot right
        CreateTile(wallTiles[7], new Vector2(botLeft.x, botLeft.y), botWall.transform); // bot left

        // Now, fill in the wall outline
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            // Top wall
            CreateTile(wallTiles[0], new Vector2(x, botLeft.y + height), topWall.transform);
            // Bot wall
            CreateTile(wallTiles[2], new Vector2(x, botLeft.y), botWall.transform);
        }

        for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
            // Left wall
            CreateTile(wallTiles[1], new Vector2(botLeft.x, y), leftWall.transform);
            // Right wall
            CreateTile(wallTiles[3], new Vector2(botLeft.x + width, y), rightWall.transform);
        }

        // Fill out the floor
        GameObject floor = new GameObject("floor");
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
                CreateTile(floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(x, y), floor.transform);
            }
        }

        // Finalize
        walls.transform.parent = floor.transform.parent = gameObject.transform;
        UpdateRoomName();
    }

    GameObject CreateTile(GameObject tilePrefab, Vector3 position, Transform parent) {
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        newTile.transform.parent = parent;
        return newTile;
    }

    public void AddDoor(Door doorToAdd) {
        doors.Add(doorToAdd);
        TintDoors(); //debug
        UpdateRoomName();
    }

    public void AddDoors(HashSet<Door> doorsToAdd) {
        doors.UnionWith(doorsToAdd);
        TintDoors(); //debug
        UpdateRoomName();
    }

    // DEBUG tints the doors blue
    private void TintDoors() {
        Transform walls = gameObject.transform.Find("walls");
        foreach (Door d in doors) {
            string sideName = d.ToString().ToLower() + "_wall";
            Transform side = walls.transform.Find(sideName);
            for (int i = 4; i < 10; i++) {
                side.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
    }

    // DEBUG tints the floor
    public void TintFloor(Color c) {
        Transform floor = gameObject.transform.Find("floor");
        for (int i = 0; i < floor.childCount; i++) {
            floor.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = c;
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
