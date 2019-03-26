using System.Collections;
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
       foreach(Sprite s in sprites)
       {
           tileSprites.Add(s.name, s);
       }
    }
}
