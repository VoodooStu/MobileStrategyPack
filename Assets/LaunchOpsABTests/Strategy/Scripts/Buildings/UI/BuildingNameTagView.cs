using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingNameTagView : MonoBehaviour
{

    bool IsShowing = false;
    IEnumerator _IShow = null;
    IEnumerator iShow()
    {
        yield return new WaitForSeconds(0.5f);
        _IShow = null;
        gameObject.SetActive(true);
    }

    IEnumerator _IHide = null;
    IEnumerator iHide()
    {

        yield return new WaitForSeconds(0.2f);
        _IHide = null;
        gameObject.SetActive(false);
    }

    public void MovingCamera()
    {
        if (!enabled)
            return;
        if (IsShowing)
            return;
        if (_IShow != null)
            StrategyMapCameraControls.Instance.StopCoroutine(_IShow);
        if (_IHide != null)
            StrategyMapCameraControls.Instance.StopCoroutine(_IHide);
        _IShow = iShow();
        StrategyMapCameraControls.Instance.StartCoroutine(_IShow);
        IsShowing = true;
        
    }

    public void StopMovingCamera()
    {
        if (!enabled)
            return;
        if (!IsShowing)
            return;
        if (_IShow != null)
            StrategyMapCameraControls.Instance.StopCoroutine(_IShow);
        if(_IHide != null)
            StrategyMapCameraControls.Instance.StopCoroutine(_IHide);
        _IHide = iHide();
        StrategyMapCameraControls.Instance.StartCoroutine(_IHide);
        IsShowing = false;
        
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
        IsShowing = true;
        StopMovingCamera();
    }
}
