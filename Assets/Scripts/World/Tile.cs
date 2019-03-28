using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    Action<Tile> onTileTypeChanged;
    public enum Type { empty, full, grass, green, wood, water_shallow, water_deep, red, white, dirt, cobble, lava, water, bedrock, mossy_cobble, acacia_plank, dark_oak_plank, birch_plank, oak_plank, jungle_plank, spruce_plank, smooth_quartz, chiseled_quartz_side, chiseled_quartz_top, quartz_pillar_side, quartz_pillar_top, quartz_block, sand, stone_double_slab, stone_slab, stone_bricks, chiseled_stone_bricks, cracked_stone_bricks, mossy_stone_bricks, andesite, polished_andesite, diorite, polished_diorite, granite, polished_granite }
    public Type type { get; private set; }
    public int x { get; private set; }
    public int y { get; private set; }
    public int z { get; private set; }
    public bool isWalkable { get; private set; }
    public bool unit { get; set; }

    public Tile(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.isWalkable = true;
        this.unit = false;

        type = Type.grass;
    }

    public Tile deepCopy()
    {
        Tile t = new Tile(this.x, this.y, this.z);
        //t.onTileTypeChanged = this.onTileTypeChanged;
        t.type = this.type;
        t.isWalkable = this.isWalkable;
        t.unit = this.unit;
        return t;
    }

    public void setTileType(Tile.Type type)
    {
        this.type = type;
        onTileTypeChanged(this);
    }

    public void registerTileTypeChanged(Action<Tile> callback)
    {
        onTileTypeChanged += callback;
    }
}
