using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingNameTagView : MonoBehaviour
{

    bool IsShowing = false;

    public void MovingCamera()
    {
        if (IsShowing)
            return;
        IsShowing = true;
        gameObject.SetActive(true);
    }

    public void StopMovingCamera()
    {
        if (!IsShowing)
            return;
        IsShowing = false;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        StrategyMapCameraControls.Instance.OnCameraMove -= MovingCamera;
        StrategyMapCameraControls.Instance.OnCameraStopMove -= StopMovingCamera;
    }

    private void Start()
    {
        StrategyMapCameraControls.Instance.OnCameraMove += MovingCamera;
        StrategyMapCameraControls.Instance.OnCameraStopMove += StopMovingCamera;
        StopMovingCamera();
    }
}
