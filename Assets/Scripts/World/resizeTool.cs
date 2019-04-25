using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Drawing selection tool : https://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/

public class resizeTool : MonoBehaviour
{
    bool isSelecting = false;
    Vector3 mousePosition1;
    static Texture2D _selectionTexture;

    public GameObject importedMap;
    public GameObject confirmation;
    private float scale;

     void Update()
    {
        // If we press the left mouse button, save mouse location and begin selection
        if( Input.GetMouseButtonDown( 0 ) )
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }
        // If we let go of the left mouse button, end selection
        if( Input.GetMouseButtonUp( 0 ) )
        {
            isSelecting = false;
            if(scale > 0 && scale != Mathf.Infinity)
                confirmation.SetActive(true);
        }
    }

    public static Texture2D selectionTexture
    {
        get
        {
            if( _selectionTexture == null )
            {
                _selectionTexture = new Texture2D( 1, 1 );
                _selectionTexture.SetPixel( 0, 0, Color.cyan );
                _selectionTexture.Apply();
            }
 
            return _selectionTexture;
        }
    }
 
    public static void DrawScreenRect( Rect rect, Color color )
    {
        GUI.color = color;
        GUI.DrawTexture( rect, selectionTexture );
        GUI.color = Color.cyan;
    }

    public static void DrawScreenRectBorder( Rect rect, float thickness, Color color )
    {
        // Top
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
        // Left
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
        // Right
        DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
        // Bottom
        DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
    }

    public static Rect GetScreenRect( Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
        var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
        // Create Rect
        return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
    }
    
    void OnGUI()
    {
        if( isSelecting )
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect( mousePosition1, Input.mousePosition );
            DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
            float diagonal3x3 = Mathf.Sqrt(18);
            if(diagonal3x3/getSelectionDiagonal() != Mathf.Infinity)
            {
                scale = diagonal3x3/getSelectionDiagonal();
                scale = (float)System.Math.Round(scale,3);
            }
        }
    }
	
    public static Bounds GetWorldBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        var v1 = Camera.main.ScreenToWorldPoint( screenPosition1 );
        var v2 = Camera.main.ScreenToWorldPoint( screenPosition2 );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;
    
        var bounds = new Bounds();
        bounds.SetMinMax( min, max );
        return bounds;
    }

    public float getSelectionDiagonal()
    {
        Camera camera = Camera.main;
        Bounds viewportBounds = GetWorldBounds( camera, mousePosition1, Input.mousePosition );
        Vector3 size = viewportBounds.size;
        float diagonalLength = Mathf.Sqrt(Mathf.Pow(size.x,2)+Mathf.Pow(size.y,2));
        return diagonalLength;
    }

    public void confirm()
    {
        isSelecting = false;
        confirmation.SetActive(false);
        World.instance.setUIInteract(true);
        scaleImage();
    }

    public void scaleImage()
    {
        Debug.Log(scale);
        importedMap.transform.localScale = new Vector3(scale, scale, 1);
    }

    public void cancel()
    {
        scale = Mathf.Infinity;
        isSelecting = false;
        confirmation.SetActive(false);
    }
}