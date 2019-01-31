using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchTest : MonoBehaviour 
{
    void Update () 
    {
        Touch[] myTouches = Input.touches;
        print(myTouches.Length + " touches detected.");
    }
}
