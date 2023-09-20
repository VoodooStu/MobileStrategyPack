using Cinemachine;
using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class StrategyMapCameraControls : MonoBehaviour
{
    public static StrategyMapCameraControls Instance =>instance;

    public Action OnCameraMove;
    public Action OnCameraStopMove;

    private static StrategyMapCameraControls instance;
    public LayerMask BuildingLayer;
    public Vector2 ZoomRestraints;
    public Vector2 CameraXRestraints;
    public Vector2 CameraZRestraints;

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

    public void SetCamera(CinemachineVirtualCamera camera)
    {
        _currentVCam.Priority =- 10;
        if(camera == _defaultVCam)
        {
            camera.transform.position = _currentVCam.transform.position;
        }
        if(camera!= _defaultVCam)
        {
            CurrentZoom = camera.m_Lens.FieldOfView;
        }

        _currentVCam.gameObject.SetActive(false);
         _currentVCam = camera;
        _currentVCam.gameObject.SetActive(true);
        _currentVCam.Priority = 10;

    }
    Vector2 lastInputPosition;
    float onMouseDown;
    BuildingController _currentBuilding;
    bool cameraIsMoving = false;
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
        bool isInputBlockedByUI = EventSystem.current.IsPointerOverGameObject();
        if (canSelect && !isInputBlockedByUI)
        {
           


            if (Input.GetMouseButtonDown(0))
            {
                onMouseDown = Time.time;
                lastInputPosition = Input.mousePosition;
               
            }
            if (Input.GetMouseButton(0))
            {
                OnCameraMove?.Invoke();
                Vector2 delta = lastInputPosition - (Vector2)Input.mousePosition;
                delta.x/= Screen.width;
                delta.y /= Screen.height;
                _currentVCam.transform.Translate(delta *CameraSpeed);
                var posiition = _currentVCam.transform.position;
                posiition.x = Mathf.Clamp(posiition.x, CameraXRestraints.x, CameraXRestraints.y);
                posiition.z = Mathf.Clamp(posiition.z, CameraZRestraints.x, CameraZRestraints.y);
                _currentVCam.transform.position = posiition;
                lastInputPosition = Input.mousePosition;
                cameraIsMoving = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (cameraIsMoving)
                {
                    cameraIsMoving = false;
                    OnCameraStopMove?.Invoke();
                }
              
            }
            if(Input.GetMouseButtonUp(0) && Time.time - onMouseDown < 0.2f&& !EventSystem.current.IsPointerOverGameObject())
            {
                OnCameraStopMove?.Invoke();
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, BuildingLayer))
                {
                    BuildingController building = hit.collider.GetComponentInParent<BuildingController>();
                    if (building != null && (building.BuildingDefinition.Level>0 || BuildingManager.Instance.IsUpgrading(building.BuildingDefinition)))
                    {
                        _currentBuilding = building;
                        _currentBuilding.Select();
                        BuildingManager.Instance.SelectBuilding(building.BuildingDefinition);
                        
                     
                    }else if(building != null && building.BuildingDefinition.Level == 0)
                    {
                        _currentBuilding = building;
                        StrategyUIManager.Instance.BuildBuildingPopUp.Fill(building.BuildingDefinition, BuildingManager.Instance.TryUpgradeBuilding);
                        StrategyUIManager.Instance.BuildBuildingPopUp.Show();
                    }
                }
            }
        }
        else
        {
            if (cameraIsMoving)
            {
                cameraIsMoving = false;
                OnCameraStopMove?.Invoke();
            }
        }
        if (isInputBlockedByUI)
        {
            onMouseDown = Time.time;
            lastInputPosition = Input.mousePosition;
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

    internal void SetSelected(BuildingController cont)
    {
        if (_currentBuilding != null)
        {
            _currentBuilding.UnSelect();
            _currentBuilding = null;
        }
        _currentBuilding = cont;
        _currentBuilding.Select();
    }

    private CinemachineVirtualCamera _currentVCam;
    public CinemachineVirtualCamera _defaultVCam;

}
