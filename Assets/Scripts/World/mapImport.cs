using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class mapImport : MonoBehaviour
{
    public static mapImport instance;
    Sprite m_Sprite;
    private GameObject grid;
    private GameObject UI;
    public GameObject instructions;
    private void Start()
    {
        instance = this;
        grid = GameObject.Find("Grid Overlay");
        UI = GameObject.Find("UI");
    }

    public void loadImage(string absolutePath)
    {
        m_Sprite = IMG2Sprite.LoadNewSprite(absolutePath);
        this.GetComponent<SpriteRenderer>().sprite = m_Sprite;
        this.transform.localScale = new Vector3(1, 1, 1);
        resizeMode();
    }

    public void resizeMode()
    {
        Vector3 imageLocation = m_Sprite.bounds.center;
        Camera.main.transform.position = new Vector3(imageLocation.x, imageLocation.y, Camera.main.transform.position.z);

        grid.SetActive(false);
        UI.SetActive(false);
        instructions.SetActive(true);

        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = false;
        (this.GetComponent("resizeTool") as MonoBehaviour).enabled = true;
    }

    public void confirm()
    {
        grid.SetActive(true);
        UI.SetActive(true);
        instructions.SetActive(false);

        (Camera.main.GetComponent("CameraHandler") as MonoBehaviour).enabled = true;
        (this.GetComponent("resizeTool") as MonoBehaviour).enabled = false;

    }
}
