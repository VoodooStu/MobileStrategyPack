using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingViewType { 
    BuildBuilding = 0,
    BuildingInfo = 1,
    UpgradeBuilding = 2,
    BuildingInfoNoComponents = 3,
    UpgradingBuilding = 4,
    CanUpgradeBuilding = 5,
    WeaponsShop = 6,
  

}


public class BuildingBaseStatusGroup : MonoBehaviour
{
    public BuildingViewType BuildingViewType;
    public List<GameObject> ObjectsToEnable = new List<GameObject>();
    public List<GameObject> ObjectsToDisable = new List<GameObject>();
    [ContextMenu("Simulate")]
    public void ActivateGroup()
    {
        SetActive(true);

    }

    public void SetActive(bool active)
    {
        foreach (var item in ObjectsToEnable)
        {
            item.SetActive(active);
        }
        foreach (var item in ObjectsToDisable)
        {
            item.SetActive(!active);
        }
    }
}
