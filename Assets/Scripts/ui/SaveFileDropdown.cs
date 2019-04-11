using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
        string m_path;
        m_path = Application.dataPath;
        m_path = m_path.Remove(m_path.Length-6);
        m_path += "saves";
        DirectoryInfo dir = new DirectoryInfo(m_path);
        FileInfo[] info = dir.GetFiles("*.dat");
        List<Dropdown.OptionData> items = new List<Dropdown.OptionData>();
        foreach (FileInfo f in info)
        {
            string save_file = f.ToString();
            string new_save = "";
            for (int i = save_file.Length; i --> 0; )
            {
                if (save_file[i].Equals('\\'))
                {
                    break;
                }
                else
                {
                    new_save = save_file[i] + new_save;
                }
            }
            // Add the option to the list
            var saveOption = new Dropdown.OptionData(new_save);
            items.Add(saveOption);
        }
        
        dropdown.AddOptions(items);

        //var saves_files = FileNamesFromFolder("../saves/");
        //dropdown.AddOptions(saves_files);
        /* 
        dropdown.ClearOptions();
        List<string> names = new List<string>() {"test"};
        dropdown.AddOptions(names);
        */
    }
}
