using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size = 10;
    public Tile[,] tiles;
    Dictionary<Tile, GameObject> tileGOMap;

    void Awake()
    {
        tileGOMap = new Dictionary<Tile, GameObject>();
        tiles = new Tile[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                tiles[i,j] = new Tile(i + (int)transform.position.x, j + (int)transform.position.y, 0);
                tiles[i,j].registerTileTypeChanged(onTileTypeChanged);
                GameObject tileGO = new GameObject("Tile (" + (i + (int)transform.position.x) + "," + (j + (int)transform.position.y) + ")");
                tileGO.transform.position = new Vector3(tiles[i,j].x, tiles[i,j].y, tiles[i,j].z);
                tileGO.transform.SetParent(this.transform, true);
                tileGO.AddComponent<SpriteRenderer>();
                tileGOMap.Add(tiles[i,j], tileGO);
                onTileTypeChanged(tiles[i,j]);
            }
        }
    }

    void onTileTypeChanged(Tile tile)
    {
        SpriteRenderer spriteRenderer = tileGOMap[tile].GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteManager.instance.getSprite(tile);
    }

    public Tile[,] getTiles()
    {
        Tile[,] chunk = new Tile[size,size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                chunk[i,j] = tiles[i,j];
            }
        }
        return chunk;
    }

    public void setTiles(Tile[,] chunk)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                tiles[i,j].setTileType(chunk[i,j].type, true);
            }
        }
        Debug.Log("Tiles set");
    }

    public Sprite getTileSprite(Tile tile)
    {
        if(tileGOMap.ContainsKey(tile))
            return tileGOMap[tile].GetComponent<SpriteRenderer>().sprite;
        
        return null;
    }
}
