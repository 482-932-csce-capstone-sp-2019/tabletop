using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleUI : MonoBehaviour
{
    public GameObject UI;
    bool status;
    // Start is called before the first frame update
    void Start()
    {
        status = false;
        UI.gameObject.SetActive(false);
    }

    public void toggle()
    {
        if(status)
        {
            status = false;
            UI.gameObject.SetActive(false);
        }
        else
        {
            status = true;
            UI.gameObject.SetActive(true);
        }
    }
}
