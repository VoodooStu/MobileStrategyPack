using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyUIManager : MonoBehaviour
{
    public BuildingStatusView BuildingStatusView;
    public BuildBuildingPopUp BuildBuildingPopUp;
    public ResourceCollectView ResourceCollectView;
    public InsufficentResourcesView InsufficentResourcesView;
    public BuildingTimerPanel UpgradeSlotsView;
    public MissionsView MissionsView;
   
    public SeasonCompletePopUp SeasonCompletePopUp;
    private static StrategyUIManager _instance;
    public static StrategyUIManager Instance { get {
            return _instance;
        
        } }
    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
    }
}
