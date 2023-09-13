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

    public int GetMaxAllowedBuildingLevel(BuildingDefinitionSO building)
    {
        if(building.BuildingType == BuildingType.Master)
        {
            return BuildingGlobalConfig.MasterBuildingUpgradePrices.Count;
        }
        else if(building.BuildingType == BuildingType.Main)
        {
            return Mathf.Min(MasterBuilding.Level,building.UpgradePrices.Count ==0? BuildingGlobalConfig.MainBuildingUpgradePrices.Count: building.UpgradePrices.Count);
        }
        else
        {
            return BuildingGlobalConfig.SubUpgradePrices.Count;
        }
    }
    internal bool IsRestricted(BuildingDefinitionSO data)
    {
        return data.Level>GetMaxAllowedBuildingLevel(data);
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
        foreach(var building in BuildingGlobalConfig.AllBuildings)
        {
            StrategyDataManager.RegisterBuilding(building);
            
        }
    }
    public BuildingGlobalConfig BuildingGlobalConfig;

    public BuildingDefinitionSO MasterBuilding => BuildingGlobalConfig.MasterBuilding;
    public List<BuildingDefinitionSO> MainBuildings =>BuildingGlobalConfig.MainBuildings;
    public List<BuildingDefinitionSO> SubBuildings => BuildingGlobalConfig.SubBuildings;

    public Action<BuildingDefinitionSO> OnBuildingUpgraded;

    private List<BuildingController> BuildingControllers = new List<BuildingController>();

    public void RegisterBuildingController(BuildingController controller)
    {
        BuildingControllers.Add(controller);
    }


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
            if(building.BuildingType != BuildingType.Sub)
            {
               SelectBuilding(building);
            }
            
        }
    }

    internal bool IsMaxedOut(BuildingDefinitionSO building)
    {
        int level = building.Level;
        if (building.BuildingType == BuildingType.Master && building.UpgradePrices.Count > 0 ? building.UpgradePrices.Count > level : BuildingGlobalConfig.MasterBuildingUpgradePrices.Count > level)
        {
            return false;
        }
        else if (building.BuildingType == BuildingType.Main && building.UpgradePrices.Count>0? building.UpgradePrices.Count>level:BuildingGlobalConfig.MainBuildingUpgradePrices.Count>level)
        {
            return false;
        }
        else if (building.BuildingType == BuildingType.Sub && building.UpgradePrices.Count > 0 ? building.UpgradePrices.Count > level : BuildingGlobalConfig.SubUpgradePrices.Count > level)
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
            return BuildingGlobalConfig.MainBuildingUpgradePrices[level].Prices;
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

        if(building.BuildingType == BuildingType.Master)
        {
            foreach(var m in BuildingGlobalConfig.MainBuildings)
            {
                if (m.MasterBuildingConstraint >= building.Level)
                {
                    requiredLevel += building.Level;
                    acquiredLevel += m.Level;
                }
            }

            return (int)(((float)acquiredLevel / (float)requiredLevel) * 100f);
        }


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
            foreach(var cont in BuildingControllers)
            {
                if(cont.BuildingDefinition == buildingDefinition)
                    StrategyMapCameraControls.Instance.SetCamera(cont.BuildingCamera);
            }
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