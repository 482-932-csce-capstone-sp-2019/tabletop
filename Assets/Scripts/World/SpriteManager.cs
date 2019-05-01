using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager instance;
    Dictionary<string, Sprite> tileSprites;
    Dictionary<string, Sprite> terrainSprites;
    Dictionary<string, Sprite> objectSprites;

    void Awake()
    {
        instance = this;
        tileSprites = new Dictionary<string, Sprite>();
        terrainSprites = new Dictionary<string, Sprite>();
        objectSprites = new Dictionary<string, Sprite>();
        loadSprites();    
    }

    // gets the Sprite based on Tile type
    public Sprite getSprite(Tile tile)
    {
        if(tile.connectToNeighbors)
        {
            string name = tile.type.ToString();
            string modifier = "";

            Tile[] neighbors = tile.getNeighbors(/*true*/);

            if(neighbors[0] != null && neighbors[0].type == tile.type)
            {
                modifier += "N";
            }
            if(neighbors[1] != null && neighbors[1].type == tile.type)
            {
                modifier += "E";
            }
            if(neighbors[2] != null && neighbors[2].type == tile.type)
            {
                modifier += "S";
            }
            if(neighbors[3] != null && neighbors[3].type == tile.type)
            {
                modifier += "W";
            }

            name += modifier;

            if(tileSprites.ContainsKey(name))
                return tileSprites[name];

            Debug.LogError("Unrecognized tile type: " + name);
            return null;
        }

        if(tileSprites.ContainsKey(tile.type.ToString()))
            return tileSprites[tile.type.ToString()];
        
        Debug.LogError("Unrecognized tile type: " + tile.type.ToString());
        return null;
    }

    public Sprite getSprite(string name)
    {
        if(tileSprites.ContainsKey(name))
        {
            return tileSprites[name];
        }
        else if(terrainSprites.ContainsKey(name))
        {
            return terrainSprites[name];
        }
        else if(objectSprites.ContainsKey(name))
        {
            return objectSprites[name];
        }
        return null;
    }

    // Load Sprites from tilesheets
    void loadSprites()
    {
       Sprite[] dungeon_sprites = Resources.LoadAll<Sprite>("dungeon");
       Sprite[] dungeon1_sprites = Resources.LoadAll<Sprite>("dungeon 1");
       Sprite [] grasslands_sprites = Resources.LoadAll<Sprite>("grasslands");
       Sprite [] sprites = Resources.LoadAll<Sprite>("tilesheet");
       Sprite [] minecraft_sprites = Resources.LoadAll<Sprite>("textures_0");
       Sprite [] terrain_sprites = Resources.LoadAll<Sprite>("Terrain");
       foreach(Sprite s in dungeon_sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in dungeon1_sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in grasslands_sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in minecraft_sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in terrain_sprites)
       {
           tileSprites.Add(s.name, s);
       }

    }
}
