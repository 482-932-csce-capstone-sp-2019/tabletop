using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

// The World class handles the whole "world's" behavior in the map editor.
public class World : MonoBehaviour
{
    // the saveFile struct holds lists of playerLocation and serializableChunk
    // structs which allow map states to be saved as binary files
    [System.Serializable]
    public struct saveFile
    {
        public List<playerLocation> players;
        public List<serializableChunk> map;
        public serializedImportedMap importedMap;
    }

    // The serializableChunk struct serializes the tiles in the world for saving
    [System.Serializable]
    public struct serializableChunk
    {
        public int x, y, size;
        public Tile[,] tiles;
    }

    // playerLocation struct specifies the id and position of a player 
    [System.Serializable]
    public struct playerLocation
    {
        public int id, x, y;
    }

    // playerLocation struct specifies the id and position of a player 
    [System.Serializable]
    public struct serializedImportedMap
    {
        public int width, height;
        public byte[] bytes;
        public float xScale, yScale;
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
    public GameObject importedMapOverlay;

    Vector3 velocity;
    Vector3 autoAdjustOffset = new Vector3(0,0,-10f);
    float smoothTime = 0.5f;

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
        player.GetComponent<unit>().tileX = 0;
		player.GetComponent<unit>().tileY = 0;
        GameObject player1 = (GameObject)Instantiate(playerPrefab, new Vector3(1.5f, 0.5f, -1), Quaternion.identity);
        player1.GetComponent<unit>().tileX = 1;
        player1.GetComponent<unit>().tileY = 0;
        GameObject player2 = (GameObject)Instantiate(playerPrefab, new Vector3(2.5f, 0.5f, -1), Quaternion.identity);
        player2.GetComponent<unit>().tileX = 2;
        player2.GetComponent<unit>().tileY = 0;
        GameObject player3 = (GameObject)Instantiate(playerPrefab, new Vector3(-.5f, 0.5f, -1), Quaternion.identity);
        player3.GetComponent<unit>().tileX = -1;
        player3.GetComponent<unit>().tileY = 0;
        GameObject player4 = (GameObject)Instantiate(playerPrefab, new Vector3(-1.5f, 0.5f, -1), Quaternion.identity);
        player4.GetComponent<unit>().tileX = -2;
        player4.GetComponent<unit>().tileY =0;
        players = new List<GameObject>();
        players.Add(player);
        players.Add(player1);
        players.Add(player2);
        players.Add(player3);
        players.Add(player4);
    }

    // Update is called once per frame
    void Update()
    {
        FindChunksToLoad();
        // runs this if it is the first pass of the Update() function
        if (firstPass)
        {
            // Add players to initial tiles
            Tile spawn = getTileAt(0,0);
            spawn.unit = true;
            Tile spawn1 = getTileAt(1, 0);
            spawn1.unit = true;
            Tile spawn2 = getTileAt(2, 0);
            spawn2.unit = true;
            Tile spawn3 = getTileAt(-1, 0);
            spawn3.unit = true;
            Tile spawn4 = getTileAt(-2, 0);
            spawn4.unit = true;
            firstPass = false;
        }
        DeleteChunks();

        // handle mouse clicks
        if(Input.GetMouseButtonDown(0))
        {
            // get mouse position and tile at that position
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
                t.setTileType(selectedTileType);
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 centerPoint = getCenterPoint() + autoAdjustOffset;
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, centerPoint, ref velocity, smoothTime);

    }

    Vector3 getCenterPoint() {
        var bounds = new Bounds(players[0].transform.position, Vector3.zero);

        for (int i= 0; i < players.Count; i++) {
            bounds.Encapsulate(players[i].transform.position);
        }

        return bounds.center;
    }
    // Loads chunks based on current camera position
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

    // create a chunk of tiles at a position
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

    // "deletes" chunks by removing them from view, but stores them 
    // to render them correctly later
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
        }
    }

    // returns the Chunk present at a position
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

    // returns the Tile present at a position (using int positions)
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

    // returns the Tile present at a position (using float positions)
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

    // moves the selected unit from its initial Tile to a new Tile
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

    // sets a Tile's type to the selected type in build mode
    public void setTileType(string type)
    {
        if(buildMode)
        {
            Tile.Type t = (Tile.Type)System.Enum.Parse( typeof(Tile.Type), type);
            selectedTileType = t;
        }
    }

    // load a map from a binary file
    public void load()
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

        // Load new chunks into existence
        if(File.Exists(path))
        {
            saveFile s = new saveFile();
            using (Stream stream = File.Open(path,FileMode.Open))
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

            serializedImportedMap mapImage = new serializedImportedMap();
            mapImage = s.importedMap;
            Texture2D tex = new Texture2D(mapImage.width, mapImage.height);
            ImageConversion.LoadImage(tex, mapImage.bytes);
            Sprite m_Sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.zero);
            importedMapOverlay.GetComponent<SpriteRenderer>().sprite = m_Sprite;
            importedMapOverlay.transform.localScale = new Vector3(mapImage.xScale, mapImage.yScale, 1);

            Debug.Log("Loaded map and players for file: [" + path + "]");
        }
        else
            Debug.Log("There was an issue with the selected save file: [" + path + "]");
    }

    // save a map as a binary file
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

        serializedImportedMap mapOverlay = new serializedImportedMap();
        Texture2D tex = importedMapOverlay.GetComponent<SpriteRenderer>().sprite.texture;
        mapOverlay.width = tex.width;
        mapOverlay.height = tex.height;
        mapOverlay.bytes = ImageConversion.EncodeToPNG(tex);
        mapOverlay.xScale = importedMapOverlay.transform.localScale.x;
        mapOverlay.yScale = importedMapOverlay.transform.localScale.y;

        saveFile s = new saveFile();
        string path = EditorUtility.SaveFilePanel("Save map", "saves", "map", "dat");
        s.players = playerLocations;
        s.map = savedChunks;
        s.importedMap = mapOverlay;
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, s);
        fs.Close();
        Debug.Log("Saved map and player locations");
    }
}
