using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unit : MonoBehaviour
{
    public int tileX;
	public int tileY;
    public Color selected = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Renderer[] models = GetComponentsInChildren<Renderer>();
		foreach ( Renderer r in models)
			r.material.color = selected;
    }
}
