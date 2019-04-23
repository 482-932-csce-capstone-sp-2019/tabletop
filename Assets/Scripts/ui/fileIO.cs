using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class fileIO : MonoBehaviour
{
    // // load a map from a binary file
    // public static string load()
    // {
    //     string path = EditorUtility.OpenFilePanel("Load map", "saves", "dat");
    //     return path;
    // }

    // // save a map as a binary file
    // public static string save()
    // {
    //     string path = EditorUtility.SaveFilePanel("Save map", "saves", "map", "dat");
    //     return path;
    // }

    // public static string loadImage()
    // {
    //     string absolutePath = EditorUtility.OpenFilePanel("Import map image", "map images", "*");
    //     return absolutePath;
    // }
    public static fileIO instance;
    public GameObject item;
    public GameObject contentGO_load;
    public GameObject contentGO_save;
    public GameObject toggleGroup_load;
    public GameObject toggleGroup_save;
    public GameObject saveInputField;

    List<string> saves;
    List<string> images;

    List<GameObject> createdListObjects;
    string fileType;

    
    void Awake()
    {
        instance = this;
        saves = new List<string>();
        images = new List<string>();
        createdListObjects = new List<GameObject>();
        getFiles("save");
        getFiles("image");
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void loadSelectedFilename()
    {
        string selectedFilename = "";

        Toggle selectedToggle = toggleGroup_load.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
        if( selectedToggle != null )
            selectedFilename = selectedToggle.GetComponentInChildren<Text>().text;

        if(selectedFilename != null && fileType != null)
        {
            if(fileType == "saves")
                World.instance.load(Application.persistentDataPath + "/saves/" + selectedFilename);
            else if(fileType == "images")
                mapImport.instance.loadImage(Application.persistentDataPath + "/maps/" + selectedFilename);
        }
    }

    void getFiles(string type)
    {
        if(type == "save")
        {
            saves.Clear();
            string savePath = Application.persistentDataPath + "/saves/";
            //Debug.Log(savePath);
            FileInfo fileInfo = new FileInfo(savePath);
            fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.
            DirectoryInfo d = new DirectoryInfo(@savePath);
            FileInfo[] Files = d.GetFiles("*.dat");
            foreach(FileInfo file in Files )
            {
                saves.Add(file.Name);
            }
        }
        else if(type == "image")
        {
            images.Clear();
            string imagePath = Application.persistentDataPath + "/maps/";
            //Debug.Log(imagePath);
            FileInfo fileInfo = new FileInfo(imagePath);
            fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.
            DirectoryInfo d = new DirectoryInfo(@imagePath);
            FileInfo[] pngFiles = d.GetFiles("*.png");
            FileInfo[] jpgFiles = d.GetFiles("*.jpg");
            foreach(FileInfo file in pngFiles )
            {
                images.Add(file.Name);
            }
            foreach(FileInfo file in jpgFiles )
            {
                images.Add(file.Name);
            }
        }
        else
        {
            Debug.Log("Invalid parameter passed");
        }
    }

    public void setSaveSelection(bool status)
    {
        if(status)
        {
            string selectedFilename = "";

            Toggle selectedToggle = toggleGroup_save.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
            if( selectedToggle != null )
                selectedFilename = selectedToggle.GetComponentInChildren<Text>().text;

            saveInputField.GetComponent<InputField>().text = selectedFilename;
        }
    }

    public void populate(string type)
    {
        getFiles("save");
        getFiles("image");

        List<string> filenames = new List<string>();
        clearGameObjects();
        if(type == "saves")
        {
            filenames = saves;
            fileType = "saves";
        }
        else if(type == "images")
        {
            filenames = images;
            fileType = "images";
        }

        GameObject option; // Create GameObject instance
        for (int i = 0; i < filenames.Count; i++)
        {
            option = (GameObject)Instantiate(item, transform);
            option.transform.SetParent(contentGO_load.transform);
            option.GetComponent<Toggle>().group = toggleGroup_load.GetComponent<ToggleGroup>();
            option.GetComponentInChildren<Text>().text = filenames[i];
            createdListObjects.Add(option);
        }
    }

    public void populateSaveMenu()
    {
        getFiles("save");
        getFiles("image");

        List<string> filenames = new List<string>();
        clearGameObjects();
        filenames = saves;
        fileType = "saves";

        GameObject option; // Create GameObject instance
        for (int i = 0; i < filenames.Count; i++)
        {
            option = (GameObject)Instantiate(item, transform);
            option.transform.SetParent(contentGO_save.transform);
            option.GetComponent<Toggle>().group = toggleGroup_save.GetComponent<ToggleGroup>();
            option.GetComponentInChildren<Text>().text = filenames[i];
            option.GetComponent<Toggle>().onValueChanged.AddListener(setSaveSelection);
            createdListObjects.Add(option);
        }
    }

    public void saveFile()
    {
        string filename = saveInputField.GetComponent<InputField>().text;
        if(filename.Trim() != "")
        {
            if(filename.Substring(Math.Max(0, filename.Length - 4)) != ".dat" )
                filename += ".dat";
            World.instance.save(Application.persistentDataPath + "/saves/" + filename);
            World.instance.closeSaveMenu();
        }
    }

    public void clearGameObjects()
    {
        foreach(var go in createdListObjects)
        {
            GameObject.Destroy(go);
        }
    }

}
