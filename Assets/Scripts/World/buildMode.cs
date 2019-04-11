using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Drawing selection tool : https://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/

public class buildMode : MonoBehaviour
{
    bool isSelecting = false;
    Vector3 mousePosition1;
    static Texture2D _selectionTexture;
    public GameObject worldGO;
    private World world;

    void Start()
    {
        world = (World) worldGO.GetComponent(typeof(World));
    }

    void Update()
    {
        if (world.buildMode)
        {
            // If we press the left mouse button, save mouse location and begin selection
            if( Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject())
            {
                isSelecting = true;
                mousePosition1 = Input.mousePosition;
                (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = false;
            }
            // If we let go of the left mouse button, end selection
            if( Input.GetMouseButtonUp( 0 ) )
            {
                world.buildSelection();
                world.deleteAllTileHighlights();

                isSelecting = false;
                (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = true;
            }

            // Highlight all objects within the selection box
            if( isSelecting )
            {
                foreach( var chunk in FindObjectsOfType<Chunk>() )
                {
                    Tile[,] tiles = chunk.tiles;
                    for (int i = 0; i < tiles.GetLength(0); i++)
                    {
                        for (int j = 0; j < tiles.GetLength(1); j++)
                        {
                            Vector2 tilePosition = new Vector2(tiles[i,j].x, tiles[i,j].y);
                            if( IsWithinSelectionBounds( tiles[i,j] ) && world.tileHighlightMap.ContainsKey(tilePosition) == false)
                            {
                                world.createTileHighlightAt((int)tilePosition.x, (int)tilePosition.y);
                            }
                            else if(!IsWithinSelectionBounds( tiles[i,j] ) && world.tileHighlightMap.ContainsKey(tilePosition))
                            {
                                world.deleteTileHighlightAt((int)tilePosition.x, (int)tilePosition.y);
                            }
                        }
                    }
                }
            }
        }
    }

    public static Texture2D selectionTexture
    {
        get
        {
            if( _selectionTexture == null )
            {
                _selectionTexture = new Texture2D( 1, 1 );
                _selectionTexture.SetPixel( 0, 0, Color.cyan );
                _selectionTexture.Apply();
            }
 
            return _selectionTexture;
        }
    }
 
    public static void DrawScreenRect( Rect rect, Color color )
    {
        GUI.color = color;
        GUI.DrawTexture( rect, selectionTexture );
        GUI.color = Color.cyan;
    }

    public static void DrawScreenRectBorder( Rect rect, float thickness, Color color )
    {
        // Top
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
        // Left
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
        // Right
        DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
        // Bottom
        DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
    }

    public static Rect GetScreenRect( Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
        var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
        // Create Rect
        return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
    }
    
    void OnGUI()
    {
        if( isSelecting )
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect( mousePosition1, Input.mousePosition );
            DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
        }
    }

    public static Bounds GetViewportBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        var v1 = Camera.main.ScreenToViewportPoint( screenPosition1 );
        var v2 = Camera.main.ScreenToViewportPoint( screenPosition2 );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;
    
        var bounds = new Bounds();
        bounds.SetMinMax( min, max );
        return bounds;
    }

    public bool IsWithinSelectionBounds( Tile tile )
    {
        if( !isSelecting )
            return false;
 
        var camera = Camera.main;
        var bounds = GetViewportBounds( camera, mousePosition1, Input.mousePosition );
        
        Vector3 center = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0.0f);
        Vector3 bottomLeft = new Vector3(tile.x, tile.y, 0.0f);
        Vector3 bottomRight = new Vector3(tile.x + 1.0f, tile.y, 0.0f);
        Vector3 topLeft = new Vector3(tile.x, tile.y + 1.0f, 0.0f);
        Vector3 topRight = new Vector3(tile.x + 1.0f, tile.y + 1.0f, 0.0f);
        return bounds.Contains(camera.WorldToViewportPoint(center)) || bounds.Contains(camera.WorldToViewportPoint(bottomLeft)) || bounds.Contains(camera.WorldToViewportPoint(bottomRight)) || bounds.Contains(camera.WorldToViewportPoint(topLeft)) || bounds.Contains(camera.WorldToViewportPoint(topRight));
    }

}