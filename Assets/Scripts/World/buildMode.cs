using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class buildMode : MonoBehaviour
{
    bool isSelecting = false;
    Vector3 mousePosition1;

    void Update()
    {
        if (World.instance.buildMode)
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
                if(World.instance.buildMenu.activeSelf)
                    World.instance.buildSelection("tile");

                else if(World.instance.terrainMenu.activeSelf)
                {
                    //World.instance.buildSelection("terrain");
                    World.instance.removeTerrainTransparency();
                }

                else if(World.instance.deleteMode)
                    World.instance.deleteSelection();

                World.instance.deleteAllTileHighlights();

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
                            if( IsWithinSelectionBounds( tiles[i,j] ) && World.instance.tileHighlightMap.ContainsKey(tilePosition) == false)
                            {
                                if(World.instance.terrainMenu.activeSelf)
                                {
                                    World.instance.createTileHighlightAt((int)tilePosition.x, (int)tilePosition.y, true);
                                    //World.instance.buildSelection("terrain");
                                }
                                else
                                {
                                    World.instance.createTileHighlightAt((int)tilePosition.x, (int)tilePosition.y);
                                }

                            }
                            else if(!IsWithinSelectionBounds( tiles[i,j] ) && World.instance.tileHighlightMap.ContainsKey(tilePosition))
                            {
                                if(World.instance.terrainMenu.activeSelf)
                                {
                                    World.instance.deleteTileHighlightAt((int)tilePosition.x, (int)tilePosition.y, true);
                                    //World.instance.buildSelection("terrain");
                                }
                                else
                                {
                                    World.instance.deleteTileHighlightAt((int)tilePosition.x, (int)tilePosition.y);
                                }
                            }
                        }
                    }
                }
            }
        }
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

        //return bounds.Contains(camera.WorldToViewportPoint(center)) || bounds.Contains(camera.WorldToViewportPoint(bottomLeft)) || bounds.Contains(camera.WorldToViewportPoint(bottomRight)) || bounds.Contains(camera.WorldToViewportPoint(topLeft)) || bounds.Contains(camera.WorldToViewportPoint(topRight));

        Sprite s = World.instance.getChunkAt(tile.x, tile.y).getTileSprite(tile);
        var v1 = Camera.main.WorldToViewportPoint( bottomLeft );
        var v2 = Camera.main.WorldToViewportPoint( topRight );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;
        var spriteBounds = new Bounds();
        spriteBounds.SetMinMax(min,max);

        return bounds.Intersects(spriteBounds);
    }
}