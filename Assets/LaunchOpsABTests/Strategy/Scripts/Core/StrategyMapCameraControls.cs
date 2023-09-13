using Cinemachine;
using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class StrategyMapCameraControls : MonoBehaviour
{
    public static StrategyMapCameraControls Instance =>instance;
    private static StrategyMapCameraControls instance;
    public LayerMask BuildingLayer;
    public Vector2 ZoomRestraints;
    public float CurrentZoom;
    public float ZoomSpeed;
    public float CameraSpeed;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one StrategyMapCameraControls in scene!");
            return;
        }
        instance = this;
        LeanTouch.OnGesture += HandleGesture;
        _currentVCam = _defaultVCam;

    }

    private void HandleGesture(List<LeanFinger> fingers)
    {
        float direction = LeanGesture.GetPinchScale(fingers);

        if (direction < 1)
        {

        }
        else if (direction > 1)
        {

        }
    }

    private void OnZoom(int direction)
    {
        CurrentZoom = Mathf.Clamp(CurrentZoom + (direction*ZoomSpeed), ZoomRestraints.x, ZoomRestraints.y);
    }

    private void SetCamera(CinemachineVirtualCamera camera)
    {
        _currentVCam.Priority =- 10;
        if(camera == _defaultVCam)
        {
            camera.transform.position = _currentVCam.transform.position;
        }
       
        _currentVCam = camera;
        _currentVCam.Priority = 10;

    }
    Vector2 lastInputPosition;
    float onMouseDown;
    BuildingController _currentBuilding;
    private void Update()
    {
        bool canZoom = _currentVCam == _defaultVCam;

        if (canZoom)
        {

            if (Input.mouseScrollDelta.y < 0)
            {
                OnZoom(-1);
            }
            if (Input.mouseScrollDelta.y > 0)
            {
                OnZoom(1);
            }
        }
        else
        {
            CurrentZoom = 60;
        }

        bool canSelect = BuildingManager.Instance.CurrentlySelectedBuilding == null;

        if (canSelect)
        {
           


            if (Input.GetMouseButtonDown(0))
            {
                onMouseDown = Time.time;
                lastInputPosition = Input.mousePosition;
               
            }
            if (Input.GetMouseButton(0))
            {
                Vector2 delta = lastInputPosition - (Vector2)Input.mousePosition;
                delta.x/= Screen.width;
                delta.y /= Screen.height;
                _currentVCam.transform.Translate(delta *CameraSpeed);
                lastInputPosition = Input.mousePosition;
            }
            if(Input.GetMouseButtonUp(0) && Time.time - onMouseDown < 0.2f)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, BuildingLayer))
                {
                    BuildingController building = hit.collider.GetComponentInParent<BuildingController>();
                    if (building != null)
                    {
                        _currentBuilding = building;
                        _currentBuilding.Select();
                        BuildingManager.Instance.SelectBuilding(building.BuildingDefinition);
                        SetCamera(building.BuildingCamera);
                        CurrentZoom = _currentVCam.m_Lens.FieldOfView;
                    }
                }
            }
        }
        else
        {

        }

        


        _currentVCam.m_Lens.FieldOfView = CurrentZoom;
    }

    private void OnDestroy()
    {
        LeanTouch.OnGesture -= HandleGesture;
    }

    internal void SetDefaultCamera()
    {
        SetCamera(_defaultVCam);
    }

    internal void UnselectBuilding()
    {
        if (_currentBuilding != null)
        {
            _currentBuilding.UnSelect();
            _currentBuilding = null;
        }
        
    }

    private CinemachineVirtualCamera _currentVCam;
    public CinemachineVirtualCamera _defaultVCam;

}
