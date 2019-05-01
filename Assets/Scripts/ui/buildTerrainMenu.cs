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

        List<Dropdown.OptionData> smallOptions = new List<Dropdown.OptionData>();
        List<Dropdown.OptionData> mediumOptions = new List<Dropdown.OptionData>();
        List<Dropdown.OptionData> largeOptions = new List<Dropdown.OptionData>();
        
        Sprite [] sprites = Resources.LoadAll<Sprite>("Terrain");

        // Loop through each sprite
        foreach (var sprite in sprites)
        {
            string spriteName = sprite.name;
            
            // Add the option to the list
            var spriteOption = new Dropdown.OptionData(spriteName, sprite);
            var width = sprite.rect.width;
            if (width == 16) {
                smallOptions.Add(spriteOption);
            } else if (width == 32) {
                mediumOptions.Add(spriteOption);
            } else if (width == 48) {
                largeOptions.Add(spriteOption);
            }
        }

        sprites = Resources.LoadAll<Sprite>("dungeon 1");

        // Loop through each sprite
        foreach (var sprite in sprites)
        {
            string spriteName = sprite.name;
            
            // Add the option to the list
            var spriteOption = new Dropdown.OptionData(spriteName, sprite);
            var width = sprite.rect.width;
            if (width == 16) {
                smallOptions.Add(spriteOption);
            } else if (width == 32) {
                mediumOptions.Add(spriteOption);
            } else if (width == 48) {
                largeOptions.Add(spriteOption);
            }        
        }

        // Add the options to the drop down box
        m_dropdown.AddOptions(smallOptions);
        m_dropdown.AddOptions(mediumOptions);
        m_dropdown.AddOptions(largeOptions);


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
