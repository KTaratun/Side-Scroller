using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public GameObject[] m_players;
    public GameObject[] m_uniqueRooms;
    public int m_height; // Height of the level
    public int m_width; // Width of the level
    public int m_numAvtiveRooms; // How many rooms we want populated
    public RoomBase[] m_rooms;
    public int[] m_roomCode;
    public List<int> m_path;
    public string m_level;

    // Use this for initialization
    void Start()
    {
        m_uniqueRooms = Resources.LoadAll<GameObject>("Rooms/" + m_level + "/");

        InitBorder(2, 2);
        InitLevelPath();
        InitRooms();

        //m_players.p (Instantiate(Resources.Load<GameObject>("Characters/Character");
        m_players[0].transform.position = m_rooms[m_path[0]].m_door.transform.position;

        AssignNeighbors();

        // Determine furthest tile away from start and place exit there
        // Place enemies and items on all other rooms
    }

    // Update is called once per frame
    void Update() {

    }

    private void InitLevelPath()
    {
        // INITIAL ROOM SETUP

        m_rooms = new RoomBase[m_width * m_height]; // Whole grid of rooms

        // Set active rooms
        int activeRooms;
        if (m_numAvtiveRooms == 0 || m_numAvtiveRooms > m_rooms.Length)
            activeRooms = Random.Range(m_rooms.Length / 2, m_rooms.Length);
        else
            activeRooms = m_numAvtiveRooms;

        m_roomCode = new int[m_rooms.Length]; // Parallel to m_rooms, this array gives the index of what room each room will be 

        int currRoom = Random.Range(0, m_rooms.Length); // Current room we are working with
        m_roomCode[currRoom] = Random.Range(1, m_uniqueRooms.Length); // Assign a unique room to current room

        // PATH GENERATION

        int[] randNeighbor = { 0, 1, 2, 3 }; // Randomize which adjacent room to look at
        m_path = new List<int>(); // In case we find a dead end, we can back track to another room we've visited. Also for debugging
        int rewind = 1; // Used while traversing the room path
        m_path.Add(currRoom);

        for (int i = 0; i < activeRooms - 1; i++) // Loop for each room after the starter room
        {
            RandomizeArray(randNeighbor);

            for (int j = 0; j < randNeighbor.Length; j++)
            {
                int roomToCheck = -1;
                RoomBase.nbors facing = 0;

                string[] roomData = m_uniqueRooms[m_roomCode[currRoom]].name.Split('-');

                if (randNeighbor[j] == (int)RoomBase.nbors.left && currRoom > 0 && (currRoom - 1) / m_width == currRoom / m_width && 
                    m_roomCode[currRoom - 1] == 0 && (int)char.GetNumericValue(roomData[1][(int)RoomBase.nbors.left]) != 0)
                {
                    roomToCheck = currRoom - 1;
                    facing = RoomBase.nbors.left;
                }
                else if (randNeighbor[j] == (int)RoomBase.nbors.top && currRoom - m_width >= 0 && 
                    m_roomCode[currRoom - m_width] == 0 && (int)char.GetNumericValue(roomData[1][(int)RoomBase.nbors.top]) != 0)
                {
                    roomToCheck = currRoom - m_width;
                    facing = RoomBase.nbors.top;
                }
                else if (randNeighbor[j] == (int)RoomBase.nbors.right && currRoom <= m_rooms.Length && (currRoom + 1) / m_width == currRoom / m_width && 
                    m_roomCode[currRoom + 1] == 0 && (int)char.GetNumericValue(roomData[1][(int)RoomBase.nbors.right]) != 0)
                {
                    roomToCheck = currRoom + 1;
                    facing = RoomBase.nbors.right;
                }
                else if (randNeighbor[j] == (int)RoomBase.nbors.bottom && currRoom + m_width < m_rooms.Length && 
                    m_roomCode[currRoom + m_width] == 0 && (int)char.GetNumericValue(roomData[1][(int)RoomBase.nbors.bottom]) != 0)
                {
                    roomToCheck = currRoom + m_width;
                    facing = RoomBase.nbors.bottom;
                }

                if (roomToCheck >= 0)
                {
                    int oldRoomCode = currRoom;
                    currRoom = roomToCheck;
                    m_roomCode[currRoom] = ValidRoomCheck(facing, (int)char.GetNumericValue(roomData[1][(int)facing]), oldRoomCode);
                    m_path.Add(currRoom);
                    rewind = 1;

                    break;
                }

                if (j == randNeighbor.Length - 1)
                {
                    currRoom = m_path[m_path.Count - rewind];
                    rewind++;
                }
            }
        }
    }

    private int ValidRoomCheck(RoomBase.nbors _facing, int _wallCode, int _roomCode)
    {
        int[] randRoom = new int[m_uniqueRooms.Length - 1];

        for (int i = 0; i < randRoom.Length; i++)
            randRoom[i] = i + 1;

        RandomizeArray(randRoom);

        for (int i = 0; i < randRoom.Length; i++)
        {
            string[] roomData = m_uniqueRooms[randRoom[i]].name.Split('-');
            int wallToCheck = (int)char.GetNumericValue(roomData[1][(int)RoomBase.InvertFacing(_facing)]);
            int[] roomsToCheck = { (int)char.GetNumericValue(roomData[2][0]), (int)char.GetNumericValue(roomData[2][2]) };

            bool fail = false;
            List<int> roomsToTake = new List<int>();
            if (roomsToCheck[0] != 1 && roomsToCheck[1] != 1)
                for (int y = 0; y < roomsToCheck[1]; y++)
                {
                    for (int x = 0; x < roomsToCheck[0]; x++)
                    {
                        if (y == 0 && x == 0)
                            x = 1;

                        int currRoomToCheck = _roomCode + x + m_width * y;
                        if (currRoomToCheck / m_width != _roomCode / (m_width + y) || m_roomCode[currRoomToCheck] != 0)
                        {
                            fail = true;
                            break;
                        }
                        roomsToTake.Add(currRoomToCheck);
                    }
                    if (fail == true)
                        break;
                    if (y == roomsToCheck[1] - 1)
                        for (int j = 0; j < roomsToTake.Count; j++)
                            m_roomCode[roomsToTake[j]] = -1;
                }

            if (wallToCheck == 0 || fail)
                continue;

            if (wallToCheck == 7 || _wallCode == 7 ||
                (wallToCheck == 2 && _wallCode == 1 || wallToCheck == 2 && _wallCode == 3) ||
                (wallToCheck == 4 && _wallCode == 3 || wallToCheck == 4 && _wallCode == 5) ||
                (wallToCheck == 6 && _wallCode == 1 || wallToCheck == 6 && _wallCode == 5) ||
                (wallToCheck == 1 && _wallCode == 2 || wallToCheck == 3 && _wallCode == 2) ||
                (wallToCheck == 3 && _wallCode == 4 || wallToCheck == 5 && _wallCode == 4) ||
                (wallToCheck == 1 && _wallCode == 6 || wallToCheck == 5 && _wallCode == 6))
                return randRoom[i];
        }

        return -1;
    }

    private void InitRooms()
    {
        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                int roomId = x + y * m_width;
                GameObject newRoom = Instantiate(m_uniqueRooms[m_roomCode[roomId]]);
                int xWidth = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileWidth;
                int yHeight = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileHeight;
    
                newRoom.transform.SetPositionAndRotation(new Vector3(x * xWidth, -y * yHeight, transform.position.z), new Quaternion());
    
                RoomBase rBase = newRoom.GetComponent<RoomBase>();
                rBase.m_x = x;
                rBase.m_y = y;
                rBase.m_id = roomId;
                rBase.InitObjects();
                m_rooms[roomId] = rBase;
            }
        }
    }

    private void RandomizeArray(int[] _array)
    {
        for (int i = 0; i < _array.Length; i++)
        {
            int rand = Random.Range(0, i);
            int temp = _array[i];
            _array[i] = _array[rand];
            _array[rand] = temp;
        }
    }

    public void AssignNeighbors()
    {
        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                RoomBase tScript = m_rooms[x + y * m_width];
    
                tScript.m_neighbors = new RoomBase[4];
    
                if (x != 0)
                    tScript.m_neighbors[(int)RoomBase.nbors.left] = m_rooms[(x + y * m_width) - 1];
                if (x < m_width - 1)
                    tScript.m_neighbors[(int)RoomBase.nbors.right] = m_rooms[(x + y * m_width) + 1];
                if (y != 0)
                    tScript.m_neighbors[(int)RoomBase.nbors.bottom] = m_rooms[(x + y * m_width) - m_width];
                if (y < m_height - 1)
                    tScript.m_neighbors[(int)RoomBase.nbors.top] = m_rooms[(x + y * m_width) + m_width];
            }
        }
    }

    public void InitBorder(int _x, int _y)
    {
        for (int y = -_y; y < m_height + _y; y++)
        {
            for (int x = -_x; x < m_width + _x; x++)
            {
                if (y >= 0 && x >= 0 && x < m_width && y < m_height)
                    x = m_width;

                GameObject newRoom = Instantiate(m_uniqueRooms[0]);
                int xWidth = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileWidth;
                int yHeight = newRoom.GetComponent<Tiled2Unity.TiledMap>().TileHeight;

                newRoom.transform.SetPositionAndRotation(new Vector3(x * xWidth, -y * yHeight, transform.position.z), new Quaternion());
            }
        }
    }
}
