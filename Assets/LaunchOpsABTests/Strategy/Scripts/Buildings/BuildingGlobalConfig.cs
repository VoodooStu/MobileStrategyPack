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

    public int MainBuildingPower;
    public int SubBuildingPower;
    public int MasterBuildingPower;

    public int GetUpgradeSlots()
    {
        return DefaultUpgradeSlots + StrategyDataManager.ExtraBuildingSlots.Value;
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

    public int GetCurrentPowerLevel()
    {
        int currentLevel = 0;

        foreach(var building in AllBuildings)
        {
            switch(building.BuildingType)
            {
                case BuildingType.Master:
                    currentLevel += building.Level * MasterBuildingPower;
                    break;
                case BuildingType.Main:
                    currentLevel += building.Level * MainBuildingPower;
                    break;
                case BuildingType.Sub:
                    currentLevel += building.Level * SubBuildingPower;
                    break;
            }
          
        }
        return currentLevel;
    }

    public int BaseUpgradeTimer = 10;
    public int UpgradeTimerPerLevel = 20;
    public int GetUpgradeTime(BuildingDefinitionSO building)
    {
        return BaseUpgradeTimer + (building.Level * UpgradeTimerPerLevel);
    }

    internal ResourceAmountGroup GetUpgradeCost(BuildingDefinitionSO building)
    {
        if (building.UpgradePrices.Count != 0)
        {
            return building.UpgradePrices[Mathf.Clamp(building.Level, 0, building.UpgradePrices.Count - 1)];
        }
        if(building.BuildingType == BuildingType.Master)
        {
            return MasterBuildingUpgradePrices[Mathf.Clamp(building.Level,0,MasterBuildingUpgradePrices.Count-1)]; 

        }
        else if(building.BuildingType == BuildingType.Main)
        {
            return MainBuildingUpgradePrices[Mathf.Clamp(building.Level, 0, MainBuildingUpgradePrices.Count - 1)];
        }
        else
        {
            return SubUpgradePrices[Mathf.Clamp(building.Level, 0, SubUpgradePrices.Count - 1)];
        }
    }

    internal int GetPowerLevel(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Master:
               return MasterBuildingPower;
               
            case BuildingType.Main:
                return MainBuildingPower;

            case BuildingType.Sub:
                return SubBuildingPower;

        }
        return 0;
    }
}
