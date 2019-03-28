﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager instance;
    Dictionary<string, Sprite> tileSprites;

    void Awake()
    {
        instance = this;
        tileSprites = new Dictionary<string, Sprite>();
        loadSprites();    
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Sprite getSprite(Tile tile)
    {
        if(tileSprites.ContainsKey(tile.type.ToString()))
        {
            return tileSprites[tile.type.ToString()];
        }
        Debug.LogError("Unrecognized tile type: " + tile.type.ToString());
        return null;
    }

    void loadSprites()
    {
       Sprite [] sprites = Resources.LoadAll<Sprite>("tilesheet");
       Sprite [] minecraft_sprites = Resources.LoadAll<Sprite>("textures_0");
    //    Sprite grass_texture = Resources.Load<Sprite>("Texture Packs/grass/Grass");
       foreach(Sprite s in sprites)
       {
           tileSprites.Add(s.name, s);
       }
       foreach(Sprite s in minecraft_sprites) {
           tileSprites.Add(s.name, s);
       }
    //    tileSprites.Add("Grass", grass_texture);
    }
}