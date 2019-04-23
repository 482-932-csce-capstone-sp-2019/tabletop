using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    Action<Tile> onTileTypeChanged;
    // this enum contains all the names of tile types that can be used in build mode
    //public enum Type { empty, full, grass, green, wood, water_shallow, water_deep, red, white, dirt, cobble, lava, water, bedrock, mossy_cobble, acacia_plank, dark_oak_plank, birch_plank, oak_plank, jungle_plank, spruce_plank, smooth_quartz, chiseled_quartz_side, chiseled_quartz_top, quartz_pillar_side, quartz_pillar_top, quartz_block, sand, stone_double_slab, stone_slab, stone_bricks, chiseled_stone_bricks, cracked_stone_bricks, mossy_stone_bricks, andesite, polished_andesite, diorite, polished_diorite, granite, polished_granite, road_, water_ }
    public enum Type { empty, grass_, road_, water_, dungeon_floor, dungeon_void }
    public Type type { get; private set; }
    public int x { get; private set; }
    public int y { get; private set; }
    public int z { get; private set; }
    public bool isWalkable { get; private set; }
    public bool unit { get; set; }
    public bool connectToNeighbors { get; private set; }
    public Tile[] getNeighbors(bool diagonals = false)
    {
        Tile[] neighbors;

        if(diagonals)
        {
            neighbors = new Tile[8];

            // N E S W NE NW SE SW
            neighbors[0] = World.instance.getTileAt(x, y + 1);
            neighbors[1] = World.instance.getTileAt(x + 1, y);
            neighbors[2] = World.instance.getTileAt(x, y - 1);
            neighbors[3] = World.instance.getTileAt(x - 1, y);

            neighbors[4] = World.instance.getTileAt(x + 1, y + 1);
            neighbors[5] = World.instance.getTileAt(x - 1, y + 1);
            neighbors[6] = World.instance.getTileAt(x + 1, y - 1);
            neighbors[7] = World.instance.getTileAt(x - 1, y - 1);
        }
        else
        {
            neighbors = new Tile[4];

            // N E S W
            neighbors[0] = World.instance.getTileAt(x, y + 1);
            neighbors[1] = World.instance.getTileAt(x + 1, y);
            neighbors[2] = World.instance.getTileAt(x, y - 1);
            neighbors[3] = World.instance.getTileAt(x - 1, y);
        }

        return neighbors;
    }

    public Tile(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.isWalkable = true;
        this.unit = false;

        // set type to grass by default
        type = Type.grass_;
    }

    public Tile deepCopy()
    {
        Tile t = new Tile(this.x, this.y, this.z);
        t.type = this.type;
        t.isWalkable = this.isWalkable;
        t.unit = this.unit;
        return t;
    }

    public void setTileType(Tile.Type type, bool loadFromFile = false)
    {
        switch(type)
        {
            case Type.road_:
                connectToNeighbors = true;
                break;

            default:
                connectToNeighbors = false;
                break;
        }

        this.type = type;
        onTileTypeChanged(this);

        Tile[] neighbors = getNeighbors(true);
        for(int i = 0; i < neighbors.Length; i++)
        {
            if(neighbors[i] != null)
                neighbors[i].onTileTypeChanged(neighbors[i]);
        }
    }

    public void registerTileTypeChanged(Action<Tile> callback)
    {
        onTileTypeChanged += callback;
    }
}
