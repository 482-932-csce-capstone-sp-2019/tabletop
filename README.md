# Tabletop Toybox

This is our Senior Capstone project, a tabletop role-playing game simulator and map editor.

# Code Documentation

## World.cs

The World class is the most important class in the codebase. It handles the whole world’s behavior in the map editor. 

### Structs

- `public struct saveFile` 
  - Fields:
    - `List<playerLocation> players`
    - `List<serializableChunk> map` 
  - This struct is used for serializing map data, which is done in order to save/load maps to/from binary files. The struct contains two Lists. The `players` list is built of `playerLocation` structs, and the `map` list is built of `serializableChunk` structs, which are described below.
- `public struct serializableChunk`
  - Fields:
    - `int x, y, size`
    - `Tile[,] tiles`
  - This struct uses the `x`, `y`, and `size` fields to store the position and size of a Chunk, and the `tiles` field is an Array of Tiles which holds the different Tiles that comprise the Chunk.
- `public struct playerLocation`
  - Fields:
    - `int id, x, y` 
  - This struct uses the `x` and `y` fields to store a player’s position, and the `id` field specifies which player the struct is referencing.

### Unity Functions

The `Awake`, `Start`, and `Update` functions are special Unity functions. Thorough documentation is provided by Unity:

- [Awake](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html)
  - Awake is called when the script instance is being loaded.
- [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html)
  - Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
- [Update](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html)
  - Update is called every frame, if the MonoBehaviour is enabled.
- [LateUpdate](https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html)
  - LateUpdate is called every frame after all Update functions have been called.

### Functions

- `Vector3 getCenterPoint()`
  - getCenterPoint returns the center of the positions of the player objects.
- `void FindChunksToLoad()`
  - FindChunksToLoad creates Chunks based on the camera position by calling the `CreateChunkAt` function.
- `void CreateChunkAt(int x, int y)`
  - CreateChunkAt creates a new Chunk at position (x, y) if one at the same position does not already exist in the `activeMap` dictionary.
- `void DeleteChunks()`
  - DeleteChunks deletes all Chunks in the activeMap Dictionary.
- `Chunk getChunkAt(int x, int y)`
  - getChunkAt returns the Chunk at position (x, y). 
- `public void buildSelection()`
  - buildSelection iterates through all tiles in tileHighlightMap and calls setTileType on them.
- `public void createTileHighlightAt(int x, int y)`
  - This function creates GameObjects that highlight the proper tiles to show that they have been selected. The object is then added to the tileHighlightMap variable for later use.
- `public void deleteTileHighlightAt(int x, int y)`
  - deleteTileHighlightAt deletes the highlight from a tile at position (x, y).
- `public void deleteAllTileHighlights()`
  - deleteAllTileHighlights iterates through tileHighlightMap and destroys each GameObject and clears the map.  
- `Tile getTileAt(int x, int y)`
  - getTileAt returns the Tile at position (x, y).
- `Tile getTileAt(float x, float y)`
  - getTileAt returns the Tile at position (x, y). This functions the same as the function above, but takes x and y as floats instead of ints.
- `public void unselectUnit()`
  - unselectUnit unselects the selected Unit.
- `public void moveUnit(Tile start, Tile end)`
  - moveUnit moves a Unit from a starting position to a final position, represented by Tiles start and end, respectively.
- `public void toggleBuildMode()`
  - toggleBuildMode toggles Build Mode on and off.
- `public void setTileType(string type)`
  - setTileType sets the type of the selected Tile to the selected tile type in Build Mode.
- `public void load()`
  - load loads a game state from a binary file.
- `public void save()`
  - save saves a game state to a binary file

## buildMode.cs

This file describes the behavior of the game when in Build Mode. Build
mode is for creating and editing maps.

### Class Variables

- `bool isSelecting = false`
- `Vector3 mousePosition1`
- `static Texture2D _selection Texture`
- `public GameObject worldGO`
- `private World world`

### Class Methods

- `void Start()`
  - Instantiates the class variable `world` to the current World.

- `void Update()`
  - Update runs once per frame. In this context, Update works to take
    user input and figure out what to do with it. 
- `public static Texture2D selectionTexture`
  - selectionTexture does something (TODO: write this one)
- `public static void DrawScreenRect(Rect rect, Color color)`
  - DrawScreenRect draws the selection area on the screen.
- `public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)` 
  - DrawScreenRectBorder draws the border of the selection area.
- `public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)`
  - GetScreenRect returns the Rectangle defined by the bounds of the screen.


## Chunk.cs
TODO: describe what a Chunk is
- `Tile[,] getTiles()`
  - getTiles returns an Array containing the Tile objects from the specified Chunk.
- `void setTiles(Tile[,] chunk)`
  - setTiles uses an Array of Tiles to set the type of all Tiles in a Chunk.
- 

## GridMesh.cs
The GridMesh class does not have any functions associated with it aside from Awake. This class generates a grid that is laid over the top of the map, showing the border between Tiles.

## IMG2Sprite.cs
The IMG2Sprite class handles loading an image and converting it to a Sprite through a series of function calls. 
- `static Texture2D LoadTexture(string FilePath)`
  - LoadTexture loads a PNG or JPG file from disk to a Texture2D object, returning the object if successful or `null` if it fails. 
- `static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)`
  - LoadNewSprite 

## mapImport.cs

## resizeTool.cs

## SpriteManage.cs

## Tile.cs

## unit.cs

# UI Scripts

## buildTileMenu.cs

## SaveFileDropdown.cs

## toggleUI.cs
