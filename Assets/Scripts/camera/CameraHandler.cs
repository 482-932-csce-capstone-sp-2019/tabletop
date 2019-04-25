/*
Script found here: https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class CameraHandler : MonoBehaviour {

    private static readonly float PanSpeed = 20f;
    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float ZoomSpeedMouse = 2.5f;
    float lerpspeed = 1f;

    private static readonly float[] BoundsX = new float[]{-100f, 100f};
    private static readonly float[] BoundsY = new float[]{-100f, 100f};
    private static readonly float[] ZoomBounds = new float[]{10f, 85f};
    
    private Camera cam;
    
    private Vector3 lastPanPosition;
    private Vector3 returnToPos;
    private int panFingerId; // Touch mode only
    
    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only

    void Awake() {
        cam = GetComponent<Camera>();
    }
    
    void Update() {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer && !EventSystem.current.IsPointerOverGameObject() && !IsPointerOverUIObject()) {
            HandleTouch();
        } else if(!EventSystem.current.IsPointerOverGameObject() && !IsPointerOverUIObject()) {
            // Comment this line out to only work with touch gestures
            HandleMouse();
        }
    }

    void LateUpdate()
    {
        HandleMouseCamera();
    }

    void HandleTouch() {
        switch(Input.touchCount) {
    
        case 1: // Panning
            wasZoomingLastFrame = false;
            
            // If the touch began, capture its position and its finger ID.
            // Otherwise, if the finger ID of the touch doesn't match, skip it.
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject()) {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
            } else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved) {
                PanCamera(touch.position);
            }
            break;
    
        case 2: // Zooming
            Vector2[] newPositions = new Vector2[]{Input.GetTouch(0).position, Input.GetTouch(1).position};
            if (!wasZoomingLastFrame) {
                lastZoomPositions = newPositions;
                wasZoomingLastFrame = true;
            } else {
                // Zoom based on the distance between the new positions compared to the 
                // distance between the previous positions.
                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                float offset = newDistance - oldDistance;
    
                ZoomCamera(offset, ZoomSpeedTouch);
    
                lastZoomPositions = newPositions;
            }
            break;
            
        default: 
            wasZoomingLastFrame = false;
            break;
        }
    }

    void HandleMouseCamera()
    {
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            returnToPos = transform.position;
        }
        else if (Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            returnCamera(Input.mousePosition);
        }
    }

    void HandleMouse() {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()) {
            lastPanPosition = Input.mousePosition;
        } else if (Input.GetMouseButton(0) || Input.GetMouseButton(1)  && !EventSystem.current.IsPointerOverGameObject()) {
            PanCamera(Input.mousePosition);
        } 

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }

    void returnCamera(Vector3 returnPos) {

        //Vector3 offset = returnToPos - returnPos;
        //Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);

        //transform.Translate(move, Space.World);
        //Vector3 pos = transform.position;
        //pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        //pos.y = Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]);
        //transform.position = pos;
        //returnToPos = returnPos;
        // Determine how much to move the camera
        //Vector3 offset = cam.ScreenToViewportPoint(transform.position - returnToPos);
        //Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);

        // Perform the movement
        //transform.Translate(move);

        // Ensure the camera remains within bounds.
        //Vector3 pos = transform.position;
        //pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        //pos.y = Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]);

        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / lerpspeed);
        }
      
        transform.position = Vector3.Lerp(returnPos, returnToPos, t);
       

        //transform.position = returnToPos;
        //Vector3 offset = cam.ScreenToViewportPoint(returnToPos - returnPos);
        //Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);

        // Perform the movement
        //transform.Translate(move, Space.World);

    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    void PanCamera(Vector3 newPanPosition) {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);
        
        // Perform the movement
        transform.Translate(move, Space.World);  
        
        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.y = Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]);
        transform.position = pos;
    
        // Cache the position
        lastPanPosition = newPanPosition;
    }
    
    void ZoomCamera(float offset, float speed) {
        if (offset == 0) {
            return;
        }
    
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}
