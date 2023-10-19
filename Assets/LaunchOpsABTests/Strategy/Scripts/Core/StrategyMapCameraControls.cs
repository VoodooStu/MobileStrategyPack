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

    public Transform DragArea;

    public CustomStandaloneInputModule inputModule = new CustomStandaloneInputModule();

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
        CurrentZoom = ZoomRestraints.y;

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
            CurrentZoom = ZoomRestraints.y;
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
    Vector2 startInputPosition;
    float onMouseDown;
    BuildingController _currentBuilding;
    bool cameraIsMoving = false;
    private void Update()
    {
        if(inputModule== null)
        {
            inputModule = new CustomStandaloneInputModule();
        }

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
           // CurrentZoom = 60;
        }

        bool canSelect = BuildingManager.Instance.CurrentlySelectedBuilding == null;
        bool isInputBlockedByUI = EventSystem.current.IsPointerOverGameObject();
        

       

        if (canSelect && !isInputBlockedByUI)
        {
           


            if (Input.GetMouseButtonDown(0))
            {
                startInputPosition = Input.mousePosition;
                onMouseDown = Time.time;
                lastInputPosition = Input.mousePosition;
               
            }
            if (Input.GetMouseButton(0))
            {
                OnCameraMove?.Invoke();
                Vector2 delta = lastInputPosition - (Vector2)Input.mousePosition;
                delta.x/= Screen.width;
                delta.y /= Screen.height;

                Vector3 pos = new Vector3(delta.x, 0, delta.y);
               
                //_currentVCam.transform.Translate(pos * CameraSpeed);
                _currentVCam.transform.position += pos*CameraSpeed;
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
            if(Input.GetMouseButtonUp(0) && Time.time - onMouseDown < 0.15f&& !EventSystem.current.IsPointerOverGameObject() && ((Vector2)Input.mousePosition - startInputPosition).magnitude <1f)
            {
              
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, float.MaxValue,BuildingLayer))
                {
                    BuildingController building = hit.collider.GetComponentInParent<BuildingController>();
                   
                    BuildingManager.Instance.SelectBuilding(building.BuildingDefinition);
                     
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
  
    public CinemachineVirtualCamera _currentVCam;
    public CinemachineVirtualCamera _defaultVCam;

}
public class CustomStandaloneInputModule : StandaloneInputModule
{
    public GameObject GetHovered()
    {
        var mouseEvent = GetLastPointerEventData(-1);
        if (mouseEvent == null)
            return null;
        return mouseEvent.pointerCurrentRaycast.gameObject;
    }

    public List<GameObject> GetAllHovered()
    {
        var mouseEvent = GetLastPointerEventData(-1);
        if (mouseEvent == null)
            return null;
        return mouseEvent.hovered;
    }
}