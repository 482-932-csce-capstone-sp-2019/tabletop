using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size = 25;
    Tile[,] tiles;

    public Sprite full;
    public Sprite empty;
    public Sprite grass;

    void Awake()
    {
        tiles = new Tile[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                tiles[i,j] = new Tile(i + (int)transform.position.x, j + (int)transform.position.y, 0);
                GameObject tileGO = new GameObject("Tile (" + (i + (int)transform.position.x) + "," + (j + (int)transform.position.y) + ")");
                tileGO.transform.position = new Vector3(tiles[i,j].x, tiles[i,j].y, tiles[i,j].z);
                tileGO.transform.SetParent(this.transform, true);
                SpriteRenderer spriteRenderer = tileGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = full;
            }
        }
    }
}
