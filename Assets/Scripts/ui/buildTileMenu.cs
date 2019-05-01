using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Localization drop down example.
/// </summary>
public class buildTileMenu : MonoBehaviour
{
    [Tooltip("The drop down box to populate with sprite placement options")]
    public Dropdown m_dropdown;

    public Dictionary<string, string> menuMapping;

    public World world;

    /// <summary>
    /// Start this instance!
    /// </summary>
    void Start()
    {
        menuMapping = new Dictionary<string, string>();
        menuMapping["grass_"] = "Grass";
        menuMapping["empty"] = "Void";
        menuMapping["road_"] = "Road";
        menuMapping["water_"] = "Water";
        menuMapping["dungeon_floor"] = "Floor";

        // Clear any existing options, just in case
        m_dropdown.ClearOptions();

        // Create the option list
        List<Dropdown.OptionData> items = new List<Dropdown.OptionData>();
        
        Sprite[] basic_sprites = Resources.LoadAll<Sprite>("tilesheet");
        Sprite[] dungeon_sprites = Resources.LoadAll<Sprite>("dungeon");
        Sprite[] overworld_sprites = Resources.LoadAll<Sprite>("grasslands");

        // Loop through each sprite
        foreach (var sprite in basic_sprites)
        {
            string spriteName = sprite.name;
            
            // Add the option to the list
            if(menuMapping.ContainsKey(spriteName))
            {
                var spriteOption = new Dropdown.OptionData(menuMapping[spriteName], sprite);
                items.Add(spriteOption);
            }
        }
        foreach (var sprite in dungeon_sprites)
        {
            string spriteName = sprite.name;
            
            // Add the option to the list
            if(menuMapping.ContainsKey(spriteName))
            {
                var spriteOption = new Dropdown.OptionData(menuMapping[spriteName], sprite);
                items.Add(spriteOption);
            }
        }
        foreach (var sprite in overworld_sprites)
        {
            string spriteName = sprite.name;
            
            // Add the option to the list
            if(menuMapping.ContainsKey(spriteName))
            {
                var spriteOption = new Dropdown.OptionData(menuMapping[spriteName], sprite);
                items.Add(spriteOption);
            }
        }

        // Add the options to the drop down box
        m_dropdown.AddOptions(items);

        //Add listener for when the value of the Dropdown changes, to take action
        m_dropdown.onValueChanged.AddListener(delegate
        {
            dropdownValueChanged(m_dropdown);
        });
    }

    void dropdownValueChanged(Dropdown change)
    {
        string selected = m_dropdown.options[change.value].text;

        foreach (var pair in menuMapping)
        {
            if(pair.Value == selected)
                selected = pair.Key;
        }
        world.setTileType(selected);
    }

    
}