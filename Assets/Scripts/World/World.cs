using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class World : MonoBehaviour
{
    [System.Serializable]
    public struct saveFile
    {
        public List<playerLocation> players;
        public List<serializableChunk> map;
    }

    [System.Serializable]
    public struct serializableChunk
    {
        public int x, y, size;
        public Tile[,] tiles;
    }

    [System.Serializable]
    public struct playerLocation
    {
        public int id, x, y;
    }

    public float renderDistance = 15;
    Dictionary<Vector2, Chunk> chunkMap;
    Dictionary<Vector2, Chunk> activeMap;
    List<serializableChunk> savedChunks;
    public GameObject chunkGO;

    public GameObject playerPrefab;
	public GameObject selectedUnit;
	List<GameObject> players;

    public bool buildMode;

    public Tile.Type selectedTileType;

    bool firstPass = true;

    string dataPath;

    void Awake()
    {
        chunkMap = new Dictionary<Vector2, Chunk>();
        activeMap = new Dictionary<Vector2, Chunk>();
        savedChunks = new List<serializableChunk>();
        buildMode = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = (GameObject)Instantiate( playerPrefab, new Vector3(0.5f, 0.5f, -1), Quaternion.identity );
		// player.GetComponent<unit>().tileX = (int)player.transform.position.x;
		// player.GetComponent<unit>().tileY = (int)player.transform.position.y;
        player.GetComponent<unit>().tileX = 0;
		player.GetComponent<unit>().tileY = 0;
		players = new List<GameObject>();
		players.Add(player);

        //dataPath = Path.Combine(Application.persistentDataPath, "map.dat");
    }

    // Update is called once per frame
    void Update()
    {
        FindChunksToLoad();
        if (firstPass)
        {
            // Add players to initial tiles
            Tile spawn = getTileAt(0,0);
            spawn.unit = true;//players[0].GetComponent<unit>();
            firstPass = false;
        }
        DeleteChunks();

        if(Input.GetMouseButtonDown(0))
        {
            // Vector3 mousePosition = Input.mousePosition;
            // mousePosition.z = 1;
            float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
            Tile t = getTileAt(mouseX, mouseY);

            if(t != null && !buildMode && !EventSystem.current.IsPointerOverGameObject())
            {
                // Select unit
                if(t.unit && selectedUnit == null)
                {
                    Debug.Log("unit selected");
                    foreach(GameObject player in players)
                    {
                        if (player.GetComponent<unit>().tileX == t.x && player.GetComponent<unit>().tileY == t.y)
                        {
                            selectedUnit = player;
                            selectedUnit.GetComponent<unit>().selected = Color.red;
                        }
                    }
                }

                // Move unit
                if(!t.unit && selectedUnit != null)
                {
                    Tile start = getTileAt(selectedUnit.transform.position.x, selectedUnit.transform.position.y);
                    moveUnit(start, t);
                }

            }
            else if (t != null && buildMode && !EventSystem.current.IsPointerOverGameObject())
            {
                // if(t.type == Tile.Type.empty)
                // {
                //     t.setTileType(Tile.Type.full);
                // }

                // else if(t.type == Tile.Type.full)
                // {
                //     t.setTileType(Tile.Type.empty);
                // }
                t.setTileType(selectedTileType);
            }
        }
    }

    void FindChunksToLoad()
    {
        int xPos = (int)transform.position.x;
        int yPos = (int)transform.position.y;
        for (int i = xPos - Chunk.size; i < xPos + (2*Chunk.size); i += Chunk.size)
        {
            for (int j = yPos - Chunk.size; j < yPos + (2*Chunk.size); j += Chunk.size)
            {
                CreateChunkAt(i, j);
            }
        }
    }

    void CreateChunkAt(int x, int y)
    {
        x = Mathf.FloorToInt(x / (float)Chunk.size) * Chunk.size;
        y = Mathf.FloorToInt(y / (float)Chunk.size) * Chunk.size;

        /*

        If doesn't exist in active or total map - instantiate and add to both

        If exists in active but not total - shouldn't be possible

        If exists in total but not active - load chunk

        If exists in both - do nothing?

         */

        // // Create new chunk
        // if(activeMap.ContainsKey(new Vector2(x,y)) == false && chunkMap.ContainsKey(new Vector2(x,y)) == false)
        // {
        //     GameObject chunk = (GameObject)Instantiate(chunkGO, new Vector3(x, y, 0f), Quaternion.identity);
        //     chunkMap.Add(new Vector2(x,y), chunk.GetComponent<Chunk>());
        //     activeMap.Add(new Vector2(x,y), chunk.GetComponent<Chunk>());
        // }

        // // Load chunk
        // else if(activeMap.ContainsKey(new Vector2(x,y)) == false && chunkMap.ContainsKey(new Vector2(x,y)) == true)
        // {
        //     Debug.Log("Loading chunk");
        //     GameObject chunk = (GameObject)Instantiate(chunkGO, new Vector3(x, y, 0f), Quaternion.identity);
        //     Chunk c = chunk.GetComponent<Chunk>();

        //     if(chunkMap[new Vector2(x,y)] != null)
        //     {
        //         c = chunkMap[new Vector2(x,y)];
        //         c.tiles[0,0].setTileType(Tile.Type.empty);
        //         activeMap.Add(new Vector2(x,y), c.GetComponent<Chunk>());
        //     }
        //     else
        //     {
        //         activeMap.Add(new Vector2(x,y), chunk.GetComponent<Chunk>());
        //     }
        //     Debug.Log("Done loading?");
        // }

        if(activeMap.ContainsKey(new Vector2(x,y)) == false)
        {
            GameObject chunk = (GameObject)Instantiate(chunkGO, new Vector3(x, y, 0f), Quaternion.identity);
            activeMap.Add(new Vector2(x,y), chunk.GetComponent<Chunk>());
        }
    }

    void DeleteChunks()
    {
        List<Chunk> deleteChunks = new List<Chunk>(activeMap.Values);
        Queue<Chunk> deleteQueue = new Queue<Chunk>();
        for (int i = 0; i < deleteChunks.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, deleteChunks[i].transform.position);
            if (distance > renderDistance * Chunk.size)
            {
                deleteQueue.Enqueue(deleteChunks[i]);
            }
        }

        while (deleteQueue.Count > 0)
        {
            Chunk chunk = deleteQueue.Dequeue();
            //activeMap.Remove(chunk.transform.position);
            // Save chunk here??
            //Destroy(chunk.gameObject);
        }
    }

    Chunk getChunkAt(int x, int y)
    {
        x = Mathf.FloorToInt(x / (float)Chunk.size) * Chunk.size;
        y = Mathf.FloorToInt(y / (float)Chunk.size) * Chunk.size;

        Vector2 chunkPosition = new Vector2(x,y);
        if(activeMap.ContainsKey(chunkPosition))
        {
            return activeMap[chunkPosition];
        }
        else
        {
            return null;
        }
    }

    Tile getTileAt(int x, int y)
    {
        Chunk chunk = getChunkAt(x,y);
        if(chunk != null)
        {
            return chunk.tiles[x - (int)chunk.transform.position.x, y - (int)chunk.transform.position.y];
        }
        else
        {
            return null;
        }
    }

    Tile getTileAt(float x, float y)
    {
        int X = Mathf.FloorToInt(x);
        int Y = Mathf.FloorToInt(y);
        Chunk chunk = getChunkAt(X,Y);
        if(chunk != null)
        {
            return chunk.tiles[X - (int)chunk.transform.position.x, Y - (int)chunk.transform.position.y];
        }
        else
        {
            return null;
        }
    }

 

	public void unselectUnit()
	{
		if (selectedUnit != null)
		{
			Debug.Log("unit deselected");
			selectedUnit.GetComponent<unit>().selected = Color.white;
		}
        selectedUnit = null;
	}

	public void moveUnit(Tile start, Tile end)
	{
		Debug.Log("Removing player from tile " + start.x + "," + start.y);
        start.unit = false;

        Debug.Log("Adding/Moving player to tile " + end.x + "," + end.y);
        selectedUnit.GetComponent<unit>().transform.position = new Vector3(end.x + 0.5f, end.y + 0.5f, -1);
        selectedUnit.GetComponent<unit>().tileX = end.x;
        selectedUnit.GetComponent<unit>().tileY = end.y;
        end.unit = true; //selectedUnit.GetComponent<unit>();
    }

    public void toggleBuildMode()
    {
        Debug.Log("Toggled build mode");
        if(buildMode)
        {
            buildMode = false;
        }
        else
        {
            buildMode = true;
        }
    }

    public void setTileType(string type)
    {
        if(buildMode)
        {
            Tile.Type t = (Tile.Type)System.Enum.Parse( typeof(Tile.Type), type);
            selectedTileType = t;
        }
    }

    public void load()//List<Chunk> chunks)
    {

        // Delete all existing chunks
        List<Chunk> deleteChunks = new List<Chunk>(activeMap.Values);
        Queue<Chunk> deleteQueue = new Queue<Chunk>();
        for (int i = 0; i < deleteChunks.Count; i++)
        {
            deleteQueue.Enqueue(deleteChunks[i]);
        }

        while (deleteQueue.Count > 0)
        {
            Chunk c = deleteQueue.Dequeue();
            activeMap.Remove(c.transform.position);
            Destroy(c.gameObject);
        }

        string path = EditorUtility.OpenFilePanel("Load map", "saves", "dat");

        // Load new chunks into existance
        if(File.Exists(path))//"map.dat"))
        {
            saveFile s = new saveFile();
            using (Stream stream = File.Open(path,FileMode.Open))//"map.dat", FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                s = (saveFile)bformatter.Deserialize(stream);
            }

            foreach (serializableChunk c in s.map)
            {
                int x = c.x;
                int y = c.y;
                Tile[,] tiles = c.tiles;
                CreateChunkAt(x, y);
                Chunk newChunk = getChunkAt(x,y);
                newChunk.setTiles(tiles);
            }

            foreach (playerLocation player in s.players)
            {
                int id = player.id;
                int x = player.x;
                int y = player.y;
                Tile end = getTileAt(x,y);
                Tile start = getTileAt(players[id].GetComponent<unit>().tileX, players[id].GetComponent<unit>().tileY);
                selectedUnit = players[id];
                selectedUnit.GetComponent<unit>().selected = Color.red;
                moveUnit(start, end);
                unselectUnit();
            }

            Debug.Log("Loaded map and players for file: [" + path + "]");
        }
        else
            Debug.Log("There was an issue with the selected save file: [" + path + "]");
    }

    public void save()
    {
        if(savedChunks != null)
            savedChunks.Clear();
        List<Chunk> chunks = new List<Chunk>(activeMap.Values);
        List<Vector2> coords = new List<Vector2>(activeMap.Keys);
        for(int i = 0; i < activeMap.Values.Count; i++)
        {
            serializableChunk c = new serializableChunk();
            
            c.x = (int)coords[i].x;
            c.y = (int)coords[i].y;

            int size = Chunk.size;

            Tile[,] clonedTiles = new Tile[size,size];
            Tile[,] tiles = chunks[i].getTiles();
            for(int x = 0; x < Chunk.size; x++)
            {
                for(int y = 0; y < Chunk.size; y++)
                {
                    clonedTiles[x,y] = tiles[x,y].deepCopy();
                }
            }
            c.tiles = clonedTiles;

            savedChunks.Add(c);
        }

        List<playerLocation> playerLocations = new List<playerLocation>();
        for(int i = 0; i < players.Count; i++)
        {
            playerLocation player = new playerLocation();
            player.id = i;
            player.x = players[i].GetComponent<unit>().tileX;
		    player.y = players[i].GetComponent<unit>().tileY;
            playerLocations.Add(player);
        }

        saveFile s = new saveFile();
        string path = EditorUtility.SaveFilePanel("Save map", "saves", "map", "dat");
        s.players = playerLocations;
        s.map = savedChunks;
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, s);
        fs.Close();
        Debug.Log("Saved map and player locations");
    }
}
