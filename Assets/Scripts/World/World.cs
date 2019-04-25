using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        public List<serializedTerrain> terrain;
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

    // playerLocation struct specifies the id and position of a player 
    [System.Serializable]
    public struct serializedTerrain
    {
        public int x, y;
        public string spriteName;
    }

    public static World instance;
    public float renderDistance = 15;
    public Dictionary<Vector2, Chunk> chunkMap;
    public Dictionary<Vector2, Chunk> activeMap;
    public Dictionary<Vector2, List<Vector2>> multiTileSprites;
    public Dictionary<Vector2, GameObject> tileHighlightMap { get; set; }
    public Dictionary<Vector2, GameObject> tileTerrainMap { get; set; }
    public GameObject tileHighlightPrefab;
    public List<serializableChunk> savedChunks;
    public GameObject chunkGO;

    public GameObject playerPrefab;
	public GameObject selectedUnit;
	public List<GameObject> players;

    public bool buildMode;
    public bool deleteMode;
    public bool invalidBuild;

    public Tile.Type selectedTileType;
    public Sprite selectedTerrain;

    bool firstPass = true;

    string dataPath;
    public GameObject importedMapOverlay;
    public GameObject buildMenu;
    public GameObject terrainMenu;
    public GameObject fileMenu;
    public GameObject saveMenu;

    //---------touch section-------------------
    public List<float> sizes;
    public List<int> ids;
    public List<Tile> positions;
    public bool artifactPlayerSelected = false;
    public float artifactID = -1;
    public int index = 0;
    public bool setArtifact = false;
    //public GameObject showUnit;
    public bool isClicked = false;
    //-----------------------------------------

    Vector3 velocity;
    Vector3 autoAdjustOffset = new Vector3(0,0,-10f);
    float smoothTime = 0.5f;

    float startTime, endTime, numClicks = 0;

    void Awake()
    {
        instance = this;
        chunkMap = new Dictionary<Vector2, Chunk>();
        activeMap = new Dictionary<Vector2, Chunk>();
        savedChunks = new List<serializableChunk>();
        buildMode = false;
        deleteMode = false;
        tileHighlightMap = new Dictionary<Vector2, GameObject>();
        tileTerrainMap = new Dictionary<Vector2, GameObject>();
        multiTileSprites = new Dictionary<Vector2, List<Vector2>>();

        buildMenu = GameObject.Find("Build Menu");
        terrainMenu = GameObject.Find("Terrain Menu");
        fileMenu = GameObject.Find("File Menu");
        saveMenu = GameObject.Find("Save Menu");

        buildMenu.SetActive(false);
        terrainMenu.SetActive(false);
        fileMenu.SetActive(false);
        saveMenu.SetActive(false);

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
        UpdateHighlightColor();
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


        if (Input.touchCount > 0)
        {
            isClicked = true;
            //float posX = 0;
            //float posY = 0;

            Touch touch = Input.GetTouch(0);
            //posX = touch.position.x;
            //posY = touch.position.y; 

            float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
            Tile t = getTileAt(mouseX, mouseY);

            if(t != null && !buildMode && !EventSystem.current.IsPointerOverGameObject() && artifactPlayerSelected == false && setArtifact == true)
            {
                // Select unit
                if(t.unit && selectedUnit == null)
                {
                    Debug.Log("unit selected");
                    foreach(GameObject player in players)
                    {
                        if (player.GetComponent<unit>().tileX == t.x && player.GetComponent<unit>().tileY == t.y)
                        {
                            artifactPlayerSelected = true;
                            saveTouch(touch);
                            selectedUnit = player;
                            selectedUnit.GetComponent<unit>().selected = Color.red;
                        }
                    }
                }
            }


            if(positions != null && setArtifact == true)
            {
                for(int n = 0; n < positions.Count; n++)
                {
                    if(t == positions[n])
                    {
                        index = n;
                        //artifactID = ids[n];
                    }
                }
            }

            if(t != null && !buildMode && !EventSystem.current.IsPointerOverGameObject() && artifactPlayerSelected == true && setArtifact == false)
            {
                // Select unit
                if(t.unit && selectedUnit == null)
                {
                    Debug.Log("unit selected");
                    foreach(GameObject player in players)
                    {
                        if (player.GetComponent<unit>().tileX == t.x && player.GetComponent<unit>().tileY == t.y && t.y == positions[index].y && t.x == positions[index].x)
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
                    positions[index] = t;
                }

            }
        }

        // handle mouse clicks
        if(Input.GetMouseButtonDown(0) && artifactPlayerSelected == false && setArtifact == false)
        {
            isClicked = true;
            // Disable camera movement script
            //(Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = false;

            // get mouse position and tile at that position
            float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
            Tile t = getTileAt(mouseX, mouseY);

            if(t != null && !buildMode && !EventSystem.current.IsPointerOverGameObject() && !IsPointerOverUIObject())
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
            else if (t != null && buildMode && !EventSystem.current.IsPointerOverGameObject() && !IsPointerOverUIObject())
            {
                //t.setTileType(selectedTileType);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            // Re-enable camera movement script
            //(Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = true;
        }
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    public void OnGUI()
     {
        
            float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;

             if (isClicked == true)
             {
                //GUI.Label(new Rect(5,5,400,100), mouseX.ToString());
             }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            //GUI.Label(new Rect(5,5,400,100), setArtifact.ToString()); //for debuging
        }


     }


    private void LateUpdate()
    {
        //startTime = 0f;
        //endTime = 0f;
        
       /* if (Input.GetMouseButtonDown(0) && startTime == 0) {
            //numClicks = 0;
            numClicks++;
            startTime = Time.time;
        }
        if (numClicks == 1 && Time.time - startTime < 0.5f && Input.GetMouseButtonDown(0)) {
            numClicks++;
        }
        if (Time.time - startTime >= 0.5f || numClicks >=2){
            numClicks = 0;
            startTime = 0;
        }*/
        if (Input.GetMouseButtonDown(2)) {
            Debug.Log("adjusting camera");
            Vector3 centerPoint = getCenterPoint() + autoAdjustOffset;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, centerPoint, 1f);
        }
        
       

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
    public void CreateChunkAt(int x, int y)
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

    public void buildSelection(string whichBuild)
    {
        if(whichBuild == "tile")
        {
            foreach( var tilePosition in tileHighlightMap.Keys )
            {
                Tile t = getTileAt(tilePosition.x, tilePosition.y);
                t.setTileType(selectedTileType);
            }
        }
        else if(whichBuild == "terrain")
        {
            // Handle large sprites
            if(selectedTerrain != null)
            {
                if(selectedTerrain.rect.width > 16 || selectedTerrain.rect.height > 16)
                {
                    float numX = selectedTerrain.rect.width/16;
                    float numY = selectedTerrain.rect.height/16;
                    int nX = Mathf.RoundToInt(numX);
                    int nY = Mathf.RoundToInt(numY);
                    Vector2 spriteTileDimensions = new Vector2(nX, nY);
                    if(tileHighlightMap.Count != 0)
                        placeLargeSprites(spriteTileDimensions);
                }
                else
                {
                    foreach( var tilePosition in tileHighlightMap.Keys )
                    {
                        placeTerrainAt(tilePosition);
                    }
                }
            }
        }
    }

    public void deleteSelection()
    {
        foreach( var tilePosition in tileHighlightMap.Keys )
        {
            removeTerrainAt(tilePosition);
        }
    }

    public void placeTerrainAt(Vector2 tilePosition)
    {
        if(!tileTerrainMap.ContainsKey(tilePosition))
        {
            GameObject terrain = new GameObject("terrain tile");
            terrain.transform.position = new Vector3(tilePosition.x, tilePosition.y, -0.1f);
            SpriteRenderer renderer = terrain.AddComponent<SpriteRenderer>();

            // Sprite resizing
            // if (selectedTerrain.rect.width == selectedTerrain.rect.height)
            // {
            //     float scale = selectedTerrain.rect.width/16;
            //     terrain.transform.localScale = new Vector3(terrain.transform.localScale.x / scale, terrain.transform.localScale.y / scale, 1.0f);
            // }
            // else
            // {
            //     Debug.Log("Sprite size mismatch");
            // }

            renderer.color = new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, 0.2f);

            renderer.sprite = selectedTerrain;
            tileTerrainMap[tilePosition] = terrain;
        }
        else
        {
            tileTerrainMap[tilePosition].name = "terrain tile";
            SpriteRenderer renderer = tileTerrainMap[tilePosition].GetComponent<SpriteRenderer>();
            renderer.sprite = selectedTerrain;
        }

    }

    public void placeSpecifiedTerrainAt(Vector2 tilePosition, Sprite specifiedTerrain)
    {
        if(!tileTerrainMap.ContainsKey(tilePosition))
        {
            GameObject terrain = new GameObject("terrain tile");
            if(specifiedTerrain == null)
                terrain.name = "placeholder";
            terrain.transform.position = new Vector3(tilePosition.x, tilePosition.y, -0.1f);
            SpriteRenderer renderer = terrain.AddComponent<SpriteRenderer>();

            // Sprite resizing
            // if (selectedTerrain.rect.width == selectedTerrain.rect.height)
            // {
            //     float scale = selectedTerrain.rect.width/16;
            //     terrain.transform.localScale = new Vector3(terrain.transform.localScale.x / scale, terrain.transform.localScale.y / scale, 1.0f);
            // }
            // else
            // {
            //     Debug.Log("Sprite size mismatch");
            // }

            renderer.color = new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, 0.2f);

            renderer.sprite = specifiedTerrain;
            tileTerrainMap[tilePosition] = terrain;
        }
        else
        {
            if(specifiedTerrain != null)
                tileTerrainMap[tilePosition].name = "terrain tile";
            else
                tileTerrainMap[tilePosition].name = "placeholder";
            SpriteRenderer renderer = tileTerrainMap[tilePosition].GetComponent<SpriteRenderer>();
            renderer.sprite = specifiedTerrain;
        }

    }

    public void placePlaceholderTerrainAt(Vector2 tilePosition)
    {
        if(!tileTerrainMap.ContainsKey(tilePosition))
        {
            GameObject placeholder = new GameObject("placeholder");
            placeholder.transform.position = new Vector3(tilePosition.x, tilePosition.y, -0.1f);
            SpriteRenderer renderer = placeholder.AddComponent<SpriteRenderer>();
            renderer.color = new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, 0.2f);
            renderer.sprite = null;
            tileTerrainMap[tilePosition] = placeholder;
        }
        else
        {
            tileTerrainMap[tilePosition].name = "placeholder";
            SpriteRenderer renderer = tileTerrainMap[tilePosition].GetComponent<SpriteRenderer>();
            renderer.sprite = null;
        }

    }

    void placeLargeSprites(Vector2 spriteTileDimensions)
    {
        Vector2 selectionDimensions = getSelectionDimensions();
        Vector2 bottomLeftTile = getBottomLeftOfSelection();

        // Can we fit at least one of these sprites into the current selection?
        if((int)selectionDimensions.x < (int)spriteTileDimensions.x || (int)selectionDimensions.y < (int)spriteTileDimensions.y)
        {
            invalidBuild = true;
        }
        else
        {
            invalidBuild = false;

            // Determine root tiles
            List<Vector2> roots = new List<Vector2>();
            for(int x = 0; x < selectionDimensions.x; x+=(int)spriteTileDimensions.x)
            {
                for(int y = 0; y < selectionDimensions.y; y+=(int)spriteTileDimensions.y)
                {
                    Vector2 root = new Vector2(bottomLeftTile.x + x, bottomLeftTile.y + y);
                    roots.Add(root);
                }
            }

            // Using roots, determine placeholder positions
            List<Vector2> placeholders = new List<Vector2>();
            foreach(var root in roots)
            {
                placeholders.Clear();
                for(int i = 0; i < spriteTileDimensions.x; i++)
                {
                    for(int j = 0; j < spriteTileDimensions.y; j++)
                    {
                        Vector2 coveredTilePosition = new Vector2(root.x + i, root.y + j);
                        if(i == 0 && j == 0)
                        {
                            // This is the root
                        }
                        else
                        {
                            placeholders.Add(coveredTilePosition); 
                        }
                    }
                }
                if(arePlaceholdersWithinSelection(placeholders))
                    multiTileSprites[root] = new List<Vector2>(placeholders);
            }

            foreach( var tilePosition in tileHighlightMap.Keys )
            {
                if(roots.Contains(tilePosition) && multiTileSprites.ContainsKey(tilePosition))
                {
                    placeTerrainAt(tilePosition);
                    foreach( var placeholder in multiTileSprites[tilePosition])
                    {
                        placePlaceholderTerrainAt(placeholder);
                    }
                }
            }
        }

    }

    public bool arePlaceholdersWithinSelection(List<Vector2> placeholders)
    {
        foreach( var placeholder in placeholders)
        {
            if(!tileHighlightMap.ContainsKey(placeholder))
                return false;
        }
        return true;
    }

    public void removeTerrainTransparency()
    {
        foreach(var terrainGO in tileTerrainMap.Values)
        {
            if(terrainGO != null)
            {
                SpriteRenderer renderer = terrainGO.GetComponent<SpriteRenderer>();
                renderer.color = new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, 1.0f);
            }
        }
    }

    public void removeTerrainAt(Vector2 tilePosition)
    {
        if(tileTerrainMap.ContainsKey(tilePosition))
        {
            if(tileTerrainMap[tilePosition].name != "placeholder" && !multiTileSprites.ContainsKey(tilePosition))
            {
                GameObject.Destroy(tileTerrainMap[tilePosition]);
                tileTerrainMap.Remove(tilePosition);
            }
            else if(multiTileSprites.ContainsKey(tilePosition))
            {
                foreach(var placeholder in multiTileSprites[tilePosition])
                {
                    if(tileTerrainMap.ContainsKey(placeholder))
                    {
                        GameObject.Destroy(tileTerrainMap[placeholder]);
                        tileTerrainMap.Remove(placeholder);
                    }
                    else
                        Debug.Log("Invalid placeholder");
                }
                GameObject.Destroy(tileTerrainMap[tilePosition]);
                tileTerrainMap.Remove(tilePosition);
                multiTileSprites.Remove(tilePosition);
            }
            else
            {
                Debug.Log("Attempting to delete multi tile sprite");
                Vector2 root = findMultiSpriteRoot(tilePosition);
                if(root.ToString() != (new Vector2(Mathf.Infinity, Mathf.Infinity)).ToString())
                {
                    Debug.Log("deleting root at: " + root.ToString());
                    foreach(var placeholder in multiTileSprites[root])
                    {
                        if(tileTerrainMap.ContainsKey(placeholder))
                        {
                            GameObject.Destroy(tileTerrainMap[placeholder]);
                            tileTerrainMap.Remove(placeholder);
                        }
                        else
                            Debug.Log("Invalid placeholder");
                    }
                    if(tileTerrainMap.ContainsKey(root))
                    {
                        GameObject.Destroy(tileTerrainMap[root]);
                        tileTerrainMap.Remove(root);
                        multiTileSprites.Remove(root);
                    }
                }
                else
                {
                    Debug.Log("Couldn't find root node");
                    GameObject.Destroy(tileTerrainMap[tilePosition]);
                    tileTerrainMap.Remove(tilePosition);
                }
            }
        }
    }

    void UpdateHighlightColor()
    {
        foreach(var highlightGO in tileHighlightMap.Values)
        {
            if(deleteMode || invalidBuild)
                highlightGO.GetComponent<Renderer>().material.SetColor("_GridColour", Color.red);
            else
                highlightGO.GetComponent<Renderer>().material.SetColor("_GridColour", Color.magenta);
        }
    }
    public void createTileHighlightAt(int x, int y, bool preview = false)
    {
        Quaternion rotation = Quaternion.Euler(0,90,-90);
        GameObject highlight = (GameObject)Instantiate(tileHighlightPrefab, new Vector3(x + 0.5f, y + 0.5f, -1.0f), rotation);
        Vector2 tilePosition = new Vector2(x,y);
        tileHighlightMap[tilePosition] = highlight;

        if(preview)
        {
            buildSelection("terrain");
        }
    }

    Vector2 getSelectionDimensions()
    {
        Vector2 bottomLeft = getBottomLeftOfSelection();
        Vector2 topRight = getTopRightOfSelection();
        Vector2 position = new Vector2(1 + topRight.x - bottomLeft.x, 1 + topRight.y - bottomLeft.y);
        return position;
    }
    Vector2 getBottomLeftOfSelection()
    {
        Vector2 initialValues = tileHighlightMap.Keys.First();
        float x = initialValues.x;
        float y = initialValues.y;
        foreach(var highlightPos in tileHighlightMap.Keys)
        {
            if(highlightPos.x < x)
                x = highlightPos.x;
            if(highlightPos.y < y)
                y = highlightPos.y;
        }
        Vector2 position = new Vector2(x,y);
        return position;
    }

    Vector2 getTopRightOfSelection()
    {
        Vector2 initialValues = tileHighlightMap.Keys.First();
        float x = initialValues.x;
        float y = initialValues.y;
        foreach(var highlightPos in tileHighlightMap.Keys)
        {
            if(highlightPos.x > x)
                x = highlightPos.x;
            if(highlightPos.y > y)
                y = highlightPos.y;
        }
        Vector2 position = new Vector2(x,y);
        return position;
    }

    Vector2 findMultiSpriteRoot(Vector2 position)
    {
        Debug.Log("Finding root for pos: " + position.ToString());

        if(tileTerrainMap.ContainsKey(position))
        {
            if(tileTerrainMap[position].name != "placeholder" && multiTileSprites.ContainsKey(position))
            {
                // Position is already a root
                return position;
            }
            else if(tileTerrainMap[position].name == "placeholder")
            {
                // Need to see which root the placeholder belongs to.
                foreach(var root in multiTileSprites)
                {
                    if(root.Value.Contains(position))
                    {
                        return root.Key;
                    }
                }
            }
            else
            {
                Debug.Log("How???");
            }
        }

        Debug.Log("Sprite root not found");
        return new Vector2(Mathf.Infinity, Mathf.Infinity);
    }

    public void deleteTileHighlightAt(int x, int y, bool preview = false)
    {
        Vector2 tilePosition = new Vector2(x,y);
        GameObject.Destroy(tileHighlightMap[tilePosition]);
        tileHighlightMap.Remove(tilePosition);

        if(preview)
        {
            removeTerrainAt(tilePosition);
        }
    }

    public void deleteAllTileHighlights()
    {
        foreach(var highlightGO in tileHighlightMap.Values)
        {
            GameObject.Destroy(highlightGO);
        }
        tileHighlightMap.Clear();
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
    public Chunk getChunkAt(int x, int y)
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
    public Tile getTileAt(int x, int y)
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
    public Tile getTileAt(float x, float y)
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

        artifactPlayerSelected = false; //to allow mouse input
        artifactID = -1; //allows for new selection of artifacts
	}

    // moves the selected unit from its initial Tile to a new Tile
	public void moveUnit(Tile start, Tile end)
	{
        if(start != null && end != null)
        {
            Debug.Log("Removing player from tile " + start.x + "," + start.y);
            start.unit = false;

            Debug.Log("Adding/Moving player to tile " + end.x + "," + end.y);
            selectedUnit.GetComponent<unit>().transform.position = new Vector3(end.x + 0.5f, end.y + 0.5f, -1);
            selectedUnit.GetComponent<unit>().tileX = end.x;
            selectedUnit.GetComponent<unit>().tileY = end.y;
            end.unit = true; //selectedUnit.GetComponent<unit>();
        }
        else if(end != null)
        {
            Debug.Log("Adding/Moving player to tile " + end.x + "," + end.y);
            selectedUnit.GetComponent<unit>().transform.position = new Vector3(end.x + 0.5f, end.y + 0.5f, -1);
            selectedUnit.GetComponent<unit>().tileX = end.x;
            selectedUnit.GetComponent<unit>().tileY = end.y;
            end.unit = true; //selectedUnit.GetComponent<unit>();
        }
        else
        {
            Debug.Log("Severe loading error");
        }
    }

    public void toggleBuildMode()
    {
        Debug.Log("Toggled build mode");
        if(buildMode)
        {
            buildMode = false;
            buildMenu.SetActive(false);
        }
        else
        {
            buildMode = true;
            buildMenu.SetActive(true);
        }
    }

    public void toggleTerrainMode()
    {
        Debug.Log("Toggled terrain mode");
        if(buildMode)
        {
            buildMode = false;
            terrainMenu.SetActive(false);
        }
        else
        {
            buildMode = true;
            terrainMenu.SetActive(true);
        }
    }

    public void toggleDeleteMode()
    {
        Debug.Log("Toggled delete mode");
        if(deleteMode)
        {
            deleteMode = false;
            buildMode = false;
        }
        else
        {
            deleteMode = true;
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

    public void setTerrain(string type)
    {
        if(buildMode)
        {
            if(type == "delete terrain")
            {
                selectedTerrain.name = "delete terrain";
            }
            else
            {
                Sprite terrain = SpriteManager.instance.getSprite(type);
                selectedTerrain = terrain;
            }
        }
    }

    public void setUIInteract(bool status)
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("UI Button");
        GameObject[] toggles = GameObject.FindGameObjectsWithTag("UI Toggle");
 
        foreach (GameObject button in buttons)
        {
            if(status)
                button.GetComponent<Button>().interactable = true;
            else
                button.GetComponent<Button>().interactable = false;
        }

        foreach (GameObject toggle in toggles)
        {
            toggle.GetComponent<Toggle>().isOn = false;

            if(status)
                toggle.GetComponent<Toggle>().interactable = true;
            else
                toggle.GetComponent<Toggle>().interactable = false;
        }
    }

    public void openLoadMenu(string type)
    {
        setUIInteract(false);
        saveMenu.SetActive(false);
        fileMenu.SetActive(true);
        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = false;

        if(type == "saves")
            fileIO.instance.populate(type);
        else if(type == "images")
            fileIO.instance.populate(type);
    }

    public void closeLoadMenu()
    {
        fileIO.instance.clearGameObjects();
        fileMenu.SetActive(false);
        setUIInteract(true);
        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = true;
    }


    public void openSaveMenu()
    {
        setUIInteract(false);
        fileMenu.SetActive(false);
        saveMenu.SetActive(true);
        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = false;

        fileIO.instance.populateSaveMenu();
    }

    public void closeSaveMenu()
    {
        fileIO.instance.clearGameObjects();
        saveMenu.SetActive(false);
        setUIInteract(true);
        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = true;
    }

    // load a map from a binary file
    public void load(string path)
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

        foreach(var tileTerrain in tileTerrainMap)
        {
            GameObject.Destroy(tileTerrain.Value);
        }
        tileTerrainMap.Clear();

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

            foreach (serializedTerrain tileTerrain in s.terrain)
            {
                Vector2 position = new Vector2(tileTerrain.x, tileTerrain.y);
                Sprite terrainSprite = SpriteManager.instance.getSprite(tileTerrain.spriteName);
                placeSpecifiedTerrainAt(position, terrainSprite);
            }
            removeTerrainTransparency();


            Debug.Log("Loaded map and players for file: [" + path + "]");
        }
        else
            Debug.Log("There was an issue with the selected save file: [" + path + "]");
    }

    // save a map as a binary file
    public void save(string path)
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
        if(importedMapOverlay.GetComponent<SpriteRenderer>().sprite != null)
        {
            Texture2D tex = importedMapOverlay.GetComponent<SpriteRenderer>().sprite.texture;
            mapOverlay.width = tex.width;
            mapOverlay.height = tex.height;
            mapOverlay.bytes = ImageConversion.EncodeToPNG(tex);
            mapOverlay.xScale = importedMapOverlay.transform.localScale.x;
            mapOverlay.yScale = importedMapOverlay.transform.localScale.y;
        }

        List<serializedTerrain> mapTerrain = new List<serializedTerrain>();
        foreach(var item in tileTerrainMap)
        {
            serializedTerrain tileTerrain = new serializedTerrain();
            tileTerrain.x = (int)item.Key.x;
            tileTerrain.y = (int)item.Key.y;
            string name = "";
            if(item.Value.GetComponent<SpriteRenderer>().sprite != null)
            {
                name = item.Value.GetComponent<SpriteRenderer>().sprite.name;
            }
            tileTerrain.spriteName = name;
            mapTerrain.Add(tileTerrain);
        }

        saveFile s = new saveFile();
        s.players = playerLocations;
        s.map = savedChunks;
        s.importedMap = mapOverlay;
        s.terrain = mapTerrain;
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, s);
        fs.Close();
        Debug.Log("Saved map and player locations");
    }

    //------touch functions--------
    public float getSize(Touch a) //get Size of a single touch
    {
        return ((a.radius + a.radiusVariance) * 2);
    }

    public int getID(Touch a) //get Touch ID
    {
        return a.fingerId;
    }

    public Vector2 getPosition(Touch a)
    {
        return a.position;
    }

    public void saveTouch(Touch a) //save current Touch
    {
        sizes.Add(getSize(a));
        if(ids == null)
        {
            ids.Add(0);
        }
        else
        {
            ids.Add(ids.Count);
        }
        float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        Tile t = getTileAt(mouseX, mouseY);
        positions.Add(t);

    }

    public void toggleArtifact()
    {
        if(!setArtifact)
        {
            setArtifact = true;
        }
        else
        {
            setArtifact = false;
        }
    }

    //-----------------------------

}
