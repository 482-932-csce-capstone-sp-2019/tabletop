using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class buildTerrainMenu : MonoBehaviour
{
    [Tooltip("The drop down box to populate with sprite placement options")]
    public Dropdown m_dropdown;

    public World world;

    /// <summary>
    /// Start this instance!
    /// </summary>
    void Start()
    {
        // Clear any existing options, just in case
        m_dropdown.ClearOptions();

        // Create the option list
        List<Dropdown.OptionData> items = new List<Dropdown.OptionData>();
        
        Sprite [] sprites = Resources.LoadAll<Sprite>("Terrain");

        // Loop through each sprite
        foreach (var sprite in sprites)
        {
            // Try and find the '.' in the sprite's name. This is used as a delimiter
            // between the country code and the name of the language
            string spriteName = sprite.name;
            
            // Add the option to the list
            var spriteOption = new Dropdown.OptionData(spriteName, sprite);
            items.Add(spriteOption);
        }

        sprites = Resources.LoadAll<Sprite>("dungeon 1");

        // Loop through each sprite
        foreach (var sprite in sprites)
        {
            // Try and find the '.' in the sprite's name. This is used as a delimiter
            // between the country code and the name of the language
            string spriteName = sprite.name;
            
            // Add the option to the list
            var spriteOption = new Dropdown.OptionData(spriteName, sprite);
            items.Add(spriteOption);
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
        world.setTerrain(selected);
    }
}
