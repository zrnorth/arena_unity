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

    // sub-transforms, for organizing 
    private Transform wallsTransform; // also holds any doors
    private Transform floorTransform;
    private Transform cornersTransform;

    // If we have already rendered a door don't attempt to render it again
    private HashSet<Door> handledDoors = new HashSet<Door>();

    // temp, for rendering doors
    private GameObject[] floorTiles;


    // Actually sets up a room with its vars & instantiates it in the game world.
    // At first, rooms are made without doors. Uses AddDoor() to add them after this is run.
    public void Setup(GameObject[] floorTiles, GameObject[] wallTiles, Vector2 botLeft, int _width, int _height, bool _isOuterRoom, Pair _boardCoords) {
        width = _width;
        height = _height;
        isOuterRoom = _isOuterRoom;
        boardCoords = _boardCoords;
        // temp
        this.floorTiles = floorTiles;

        // Position us correctly in the scene
        gameObject.transform.position = new Vector2((botLeft.x + width / 2), (botLeft.y + height / 2));

        // Create our sub-transforms to stay organized
        wallsTransform = new GameObject("walls").transform;
        floorTransform = new GameObject("floor").transform;
        cornersTransform = new GameObject("corners").transform;

        // Create the walls
        Transform topWallTransform = new GameObject("top_wall").transform;
        Transform botWallTransform = new GameObject("bot_wall").transform;
        Transform leftWallTransform = new GameObject("left_wall").transform;
        Transform rightWallTransform = new GameObject("right_wall").transform;

        topWallTransform.parent = botWallTransform.parent = leftWallTransform.parent = rightWallTransform.parent = wallsTransform;

        // first, the corners
        CreateTile(wallTiles[4], new Vector2(botLeft.x, botLeft.y + height), cornersTransform); // Top left
        CreateTile(wallTiles[5], new Vector2(botLeft.x + width, botLeft.y + height), cornersTransform); // Top right
        CreateTile(wallTiles[6], new Vector2(botLeft.x + width, botLeft.y), cornersTransform); // bot right
        CreateTile(wallTiles[7], new Vector2(botLeft.x, botLeft.y), cornersTransform); // bot left

        // Now, fill in the wall outline
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            // Top wall
            CreateTile(wallTiles[0], new Vector2(x, botLeft.y + height), topWallTransform);
            // Bot wall
            CreateTile(wallTiles[2], new Vector2(x, botLeft.y), botWallTransform);
        }

        for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
            // Left wall
            CreateTile(wallTiles[1], new Vector2(botLeft.x, y), leftWallTransform);
            // Right wall
            CreateTile(wallTiles[3], new Vector2(botLeft.x + width, y), rightWallTransform);
        }

        // Fill out the floor
        for (int x = (int)botLeft.x + 1; x <= (int)botLeft.x + width - 1; x++) {
            for (int y = (int)botLeft.y + 1; y <= (int)botLeft.y + height - 1; y++) {
                CreateTile(floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(x, y), floorTransform);
            }
        }

        // Finalize
        wallsTransform.parent = floorTransform.parent = cornersTransform.parent = gameObject.transform;
        UpdateRoomName();
    }

    GameObject CreateTile(GameObject tilePrefab, Vector3 position, Transform parent) {
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        newTile.transform.parent = parent;
        return newTile;
    }

    public void AddDoor(Door doorToAdd) {
        doors.Add(doorToAdd);
        HandleDoors(); 
        UpdateRoomName();
    }

    public void AddDoors(HashSet<Door> doorsToAdd) {
        doors.UnionWith(doorsToAdd);
        HandleDoors(); 
        UpdateRoomName();
    }

    // Renders doors on each wall (if they exist)
    void HandleDoors() {
        foreach (Door d in doors) {
            if (handledDoors.Contains(d)) {
                continue;
            }
            Transform side = wallsTransform.Find(d.ToString().ToLower() + "_wall");
            int middleIndex = Mathf.FloorToInt((side.childCount - 1) / 2);
            for (int i = middleIndex - 1; i <= middleIndex + 1; i++) {
                GameObject wallToReplace = side.GetChild(i).gameObject;
                GameObject doorTile = CreateTile(floorTiles[0], wallToReplace.transform.position, side);
                doorTile.transform.SetSiblingIndex(i);
                DestroyImmediate(wallToReplace);
            }
            handledDoors.Add(d);
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
    void UpdateRoomName() {
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
