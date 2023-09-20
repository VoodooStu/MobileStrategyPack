using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
            return Mathf.Min(MasterBuilding.Level-1,building.UpgradePrices.Count ==0? BuildingGlobalConfig.MainBuildingUpgradePrices.Count: building.UpgradePrices.Count);
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
        BuildingsBeingUpgraded = BuildingGlobalConfig.AllBuildings.FindAll((u) => u.ShouldUpgrade);
        CheckForUgrade();
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


    public List<BuildingDefinitionSO> BuildingsBeingUpgraded = new List<BuildingDefinitionSO>();

    public Action<BuildingDefinitionSO> OnBuildingUpgraded;
    public Action<BuildingDefinitionSO> OnBuildingBeginsUpgrade;

    private List<BuildingController> BuildingControllers = new List<BuildingController>();

    private void Update()
    {
        CheckForUgrade();
    }

    private void CheckForUgrade()
    {
        for(int i = 0;i< BuildingsBeingUpgraded.Count;i++) 
        {
            var building = BuildingsBeingUpgraded[i];
            if (!IsUpgrading(building))
            {
                BuildingsBeingUpgraded.RemoveAt(i);
                StrategyDataManager.UpgradeBuildingLevel(building.ID);
                building.ShouldUpgrade = false;
                i--;
                OnBuildingUpgraded?.Invoke(building);
            }
        }
    }

    public void RegisterBuildingController(BuildingController controller)
    {
        BuildingControllers.Add(controller);
    }


    public BuildingDefinitionSO CurrentlySelectedBuilding;
    public int GetSubBuildingUpgradePercentage(BuildingDefinitionSO main, BuildingDefinitionSO sub)
    {
        int mainLevel = main.Level;
        int subLevel = sub.Level;
        if(mainLevel == 0)
        {
            return 0;
        }
        int minSubLevel = (mainLevel - 1) * BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel;
        if(main.GetLevelRestriction(sub) == main.Level)
        {
            return (int)(((float)(subLevel) / (mainLevel*(float)BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel)) * 100);

        }
        return (int)(((float)(subLevel - minSubLevel) / (float)BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel) * 100);
    }



    internal void TryUpgradeBuilding(BuildingDefinitionSO building)
    {
        if(building.BuildingType != BuildingType.Sub && BuildingsBeingUpgraded.Count >= BuildingGlobalConfig.GetUpgradeSlots())
        {
            StrategyUIManager.Instance.UpgradeSlotsView.Show(null);

            return;
        }

       List<ResourceAmount> price = GetUpgradePrice(building);
        if (StrategyDataManager.CanAfford(price))
        {
            StrategyDataManager.Spend(price);
         
            if(building.BuildingType != BuildingType.Sub)
            {
                building.ShouldUpgrade = true;
                building.LastUpgradeTime = DateTime.UtcNow;
                OnBuildingBeginsUpgrade?.Invoke(building);
                BuildingsBeingUpgraded.Add(building);
                OnBuildingBeginsUpgrade?.Invoke(building);
            }
            else
            {
                StrategyDataManager.UpgradeBuildingLevel(building.ID);
                OnBuildingUpgraded?.Invoke(building);
            }
           
            if(building.BuildingType != BuildingType.Sub)
            {
               SelectBuilding(building);
            }

        }
        else
        {
            StrategyUIManager.Instance.InsufficentResourcesView.Show();
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
                if (m.MasterBuildingConstraint <= building.Level)
                {
                    if(m.MasterBuildingConstraint == building.Level)
                    {
                        requiredLevel += building.Level;
                        acquiredLevel += m.Level;
                    }
                    else
                    {
                        requiredLevel += 1 ;
                        acquiredLevel += m.Level-(building.Level-1);
                    }

                    
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
        if(buildingDefinition!= null && buildingDefinition.BuildingType == BuildingType.Sub)
        {
            var building = FindParentBuilding(buildingDefinition);
            if (building != null)
            {
                SelectBuilding(building);
                StrategyUIManager.Instance.BuildingStatusView.SelectSubBuilding(buildingDefinition);

            }
            return;
        }
        CurrentlySelectedBuilding = buildingDefinition;
        if (buildingDefinition != null)
        {
            foreach(var cont in BuildingControllers)
            {
                if(cont.BuildingDefinition == buildingDefinition)
                {
                    cont.Select();
                    
                    StrategyMapCameraControls.Instance.SetCamera(cont.BuildingCamera);
                    StrategyMapCameraControls.Instance.SetSelected(cont);

                }
            }
            StrategyUIManager.Instance.BuildingStatusView.Fill(buildingDefinition);
            StrategyUIManager.Instance.BuildingStatusView.Show();
        }
        else
        {
            StrategyMapCameraControls.Instance.UnselectBuilding();
        }
    }

    private BuildingDefinitionSO FindParentBuilding(BuildingDefinitionSO buildingDefinition)
    {
        foreach(var building in MainBuildings)
        {
            if (building.HasConstraint(buildingDefinition))
            {
                return building;
            }
            
        }
        return null;
    }

    public bool IsUpgrading(BuildingDefinitionSO building)
    {
        ResourceAmountGroup group = BuildingGlobalConfig.GetUpgradeCost(building);
        DateTime lastUpgrade = building.LastUpgradeTime;
        int timeNeeded = BuildingGlobalConfig.DefaultUpgradeTimer;
        ResourceAmount amount = group.Prices.Find((u)=>u.type == ResourceType.Time);
        if (!building.ShouldUpgrade)
        {
            return false;
        }
        if (amount != null)
        {
              timeNeeded = (int)amount.amount;
        }
        if(lastUpgrade == DateTime.MinValue)
        {
            return false;
        }
        if (lastUpgrade.AddSeconds(timeNeeded) > DateTime.UtcNow)
        {
            return true;
        }
        return false;

    }

    internal int GetUpgradeTime(BuildingDefinitionSO data)
    {
        var price = BuildingGlobalConfig.GetUpgradeCost(data).Prices.Find((u)=>u.type == ResourceType.Time);

        if(price == null)
        {
            return BuildingGlobalConfig.DefaultUpgradeTimer;
        }
        else
        {
            return (int)price.amount;
        }
    }

    internal void CancelUpgrade(BuildingDefinitionSO data)
    {
        BuildingsBeingUpgraded.Remove(data);
        data.ShouldUpgrade = false;
      //  data.LastUpgradeTime = DateTime.MinValue;
        OnBuildingBeginsUpgrade?.Invoke(data);
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
