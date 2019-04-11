using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileDropdown : MonoBehaviour
{
    public Dropdown dropdown;
    
    public void Dropdown_IndexChange(int index)
    {
        
    }

    void Start() 
    {
        PopulateList();
    }

    void PopulateList()
    {
        dropdown.ClearOptions();
        List<string> names = new List<string>() {"test"};
        dropdown.AddOptions(names);
    }
}
