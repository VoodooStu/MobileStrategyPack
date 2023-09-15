using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuildingGlobalConfig", menuName = "LaunchOpsABTests/Strategy/BuildingGlobalConfig")]
public class BuildingGlobalConfig : ScriptableObject
{
    public int MaxBuildingLevel;
    public int UpgradeRestrictionLevel;
    public int DefaultUpgradeSlots = 1;

    public int GetUpgradeSlots()
    {
        return DefaultUpgradeSlots;
    }

    public BuildingDefinitionSO MasterBuilding;
    public List<ResourceAmountGroup> MasterBuildingUpgradePrices = new List<ResourceAmountGroup>();

    public List<BuildingDefinitionSO> MainBuildings = new List<BuildingDefinitionSO>();
    public List<ResourceAmountGroup> MainBuildingUpgradePrices = new List<ResourceAmountGroup>();
    public List<BuildingDefinitionSO> SubBuildings = new List<BuildingDefinitionSO>();
    public List<ResourceAmountGroup> SubUpgradePrices = new List<ResourceAmountGroup>();
    public List<BuildingDefinitionSO> AllBuildings
    {
        get
        {
            List<BuildingDefinitionSO> buildings = new List<BuildingDefinitionSO>();
            buildings.Add(MasterBuilding);
            buildings.AddRange(MainBuildings);
            buildings.AddRange(SubBuildings);
            return buildings;
        }
    
    }

    public int DefaultUpgradeTimer = 10;

    internal ResourceAmountGroup GetUpgradeCost(BuildingDefinitionSO building)
    {
        if (building.UpgradePrices.Count != 0)
        {
            return building.UpgradePrices[building.Level];
        }
        if(building.BuildingType == BuildingType.Master)
        {
            return MasterBuildingUpgradePrices[building.Level]; 

        }
        else if(building.BuildingType == BuildingType.Main)
        {
            return MainBuildingUpgradePrices[building.Level];
        }
        else
        {
            return SubUpgradePrices[building.Level];
        }
    }
}
