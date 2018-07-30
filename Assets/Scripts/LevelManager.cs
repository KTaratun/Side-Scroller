using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public RoomBase[] m_uniqueRooms;
    public int m_height; // Height of the level
    public int m_width; // Width of the level
    public RoomBase[] m_rooms;

    // Use this for initialization
    void Start ()
    {
        m_height = 10;
        m_width = 10;

        int numRooms = Random.Range(15, 20);

        RoomBase[] rooms = new RoomBase[numRooms];

        InitRooms();

        // Place random start tile
        
        // Determine all potential edges
        // Place random room on randomly selected edge
        // Repeat.

        // Determine furthest tile away from start and place exit there
        // Place enemies and items on all other rooms
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitRooms()
    {
        Renderer r = GetComponent<Renderer>();

        m_rooms = new RoomBase[m_width * m_height];

        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject newRoom = Instantiate(Resources.Load<GameObject>("OBJs/Rooms/object test"));
                int xWidth = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileWidth;
                int yHeight = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileHeight;

                newRoom.transform.SetPositionAndRotation(new Vector3(x * xWidth, -y * yHeight, transform.position.z), new Quaternion());

                RoomBase rBase = newRoom.GetComponent<RoomBase>();
                rBase.m_x = x;
                rBase.m_y = y;
                rBase.m_id = x + y * m_width;
                m_rooms[x + y * m_width] = rBase;
            }
        }
    }
}
