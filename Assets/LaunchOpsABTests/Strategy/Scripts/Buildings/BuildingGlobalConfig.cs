using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuildingGlobalConfig", menuName = "LaunchOpsABTests/Strategy/BuildingGlobalConfig")]
public class BuildingGlobalConfig : ScriptableObject
{
    public int MaxBuildingLevel;
    public int UpgradeRestrictionLevel;
    public List<ResourceAmountGroup> UpgradePrices = new List<ResourceAmountGroup>();
    public List<ResourceAmountGroup> SubUpgradePrices = new List<ResourceAmountGroup>();
}
