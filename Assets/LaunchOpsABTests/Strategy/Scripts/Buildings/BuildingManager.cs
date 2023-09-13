using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Hardware;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{

    private static BuildingManager _instance;
    public static BuildingManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        RegisterBuildings();
    }

    private void RegisterBuildings()
    {
        foreach(var building in Buildings)
        {
            StrategyDataManager.RegisterBuilding(building);
            foreach(var subBuilding in building.UpgradeConstraints)
            {
                StrategyDataManager.RegisterBuilding(subBuilding.BuildingDefinition);
            }
        }
    }
    public BuildingGlobalConfig BuildingGlobalConfig;
    public List<BuildingDefinitionSO> Buildings = new List<BuildingDefinitionSO>();
    public Action<BuildingDefinitionSO> OnBuildingUpgraded;

    public BuildingDefinitionSO CurrentlySelectedBuilding;
    public int GetSubBuildingUpgradePercentage(BuildingDefinitionSO main, BuildingDefinitionSO sub)
    {
        int mainLevel = main.Level;
        int subLevel = sub.Level;

        int minSubLevel = (mainLevel - 1) * BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel;
        if(main.GetLevelRestriction(sub) == main.Level)
        {
            return (int)(((float)(subLevel) / (mainLevel*(float)BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel)) * 100);

        }
        return (int)(((float)(subLevel - minSubLevel) / (float)BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel) * 100);
    }

    internal void TryUpgradeBuilding(BuildingDefinitionSO building)
    {
       List<ResourceAmount> price = GetUpgradePrice(building);
        if (StrategyDataManager.CanAfford(price))
        {
            StrategyDataManager.Spend(price);
            StrategyDataManager.UpgradeBuildingLevel(building.ID);
            OnBuildingUpgraded?.Invoke(building);
        }
    }

    internal bool IsMaxedOut(BuildingDefinitionSO building)
    {
        int level = building.Level;
        if (level < building.UpgradePrices.Count)
        {
            return false;
        }
        else if (building.BuildingType == BuildingType.Main && BuildingGlobalConfig.UpgradePrices.Count>level)
        {
            return false;
        }
        else if (building.BuildingType == BuildingType.Sub && BuildingGlobalConfig.SubUpgradePrices.Count > level)
        {
            return false;
        }
        return true;
    }

    internal List<ResourceAmount> GetUpgradePrice(BuildingDefinitionSO building)
    {
        int level = building.Level;
        if (level < building.UpgradePrices.Count)
        {
            return building.UpgradePrices[level].Prices;
        }
        else if(building.BuildingType == BuildingType.Main)
        {
            return BuildingGlobalConfig.UpgradePrices[level].Prices;
        }
        else
        {
            return BuildingGlobalConfig.SubUpgradePrices[level].Prices;
        }
    }


    internal int GetMainBuildingUpgradePercentage(BuildingDefinitionSO building)
    {
        int requiredLevel = 0;
        int acquiredLevel = 0;
        int numberOfSubBuildings = building.UpgradeConstraints.Count;

        foreach (var constraint in building.UpgradeConstraints)
        {
            if (constraint.StartLevel >building.Level)
            {
                numberOfSubBuildings--;
            }
            else
            {
                if(constraint.StartLevel == building.Level)
                {
                    requiredLevel += building.Level * BuildingGlobalConfig.UpgradeRestrictionLevel;
                    acquiredLevel += constraint.BuildingDefinition.Level;


                }
                else
                {
                    requiredLevel += building.Level * BuildingGlobalConfig.UpgradeRestrictionLevel - (BuildingGlobalConfig.UpgradeRestrictionLevel * (building.Level - 1));
                    acquiredLevel += constraint.BuildingDefinition.Level - (BuildingGlobalConfig.UpgradeRestrictionLevel * (building.Level - 1));


                }


            }
        }

        return (int)(((float)acquiredLevel / (float)requiredLevel)*100f);


        return (int)(((float)(acquiredLevel - (numberOfSubBuildings*BuildingGlobalConfig.UpgradeRestrictionLevel * (building.Level-1))) / (float)(requiredLevel - (numberOfSubBuildings * BuildingGlobalConfig.UpgradeRestrictionLevel*(building.Level-1))) * 100));
    }

    internal bool CanUpgradeMainBuilding(BuildingDefinitionSO building)
    {
     

        return GetMainBuildingUpgradePercentage(building) >= 100;
    }

    internal bool CanUpgradSubBuildinge(BuildingDefinitionSO main, BuildingDefinitionSO sub)
    {
        int mainLevel = main.Level;
        int subLevel = sub.Level;

        int maxSubLevel = (mainLevel) * BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel;
        return maxSubLevel > subLevel;
    }

    internal void SelectBuilding(BuildingDefinitionSO buildingDefinition)
    {
      
        CurrentlySelectedBuilding = buildingDefinition;
        if (buildingDefinition != null)
        {

            StrategyUIManager.Instance.BuildingStatusView.Fill(buildingDefinition);
            StrategyUIManager.Instance.BuildingStatusView.Show();
        }
        else
        {
            StrategyMapCameraControls.Instance.UnselectBuilding();
        }
    }
}
[Serializable]
public class ResourceAmount
{
    public ResourceType type;
    public float amount;
}

[Serializable]

public class ResourceAmountGroup
{
    public List<ResourceAmount> Prices;
}