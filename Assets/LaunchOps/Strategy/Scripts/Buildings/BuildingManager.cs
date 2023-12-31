using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    /// <summary>
    /// Gets the max allowed building level, this does not include a restriction on the main building level depending on the master building level
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    internal int GetMaxBuildingLevel(BuildingDefinitionSO building)
    {
        if (building.BuildingType == BuildingType.Master)
        {
            return building.UpgradePrices.Count == 0 ? BuildingGlobalConfig.MasterBuildingUpgradePrices.Count : building.UpgradePrices.Count;
           
        }
        else if (building.BuildingType == BuildingType.Main)
        {
            return building.UpgradePrices.Count==0?BuildingGlobalConfig.MainBuildingUpgradePrices.Count : building.UpgradePrices.Count;
        }
        else
        {
            return building.UpgradePrices.Count == 0 ? BuildingGlobalConfig.SubUpgradePrices.Count : building.UpgradePrices.Count;
           
        }
    }
    /// <summary>
    /// Gets the maxed allowed building level, this also includes a restriction on the main building level depending on the master building level
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
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


    private List<BuildingController> BuildingControllers = new List<BuildingController>();
    public BuildingDefinitionSO CurrentlySelectedBuilding =>CurrentlySelectedBuildingController?.BuildingDefinition;
    public BuildingController CurrentlySelectedBuildingController;

    

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
              
                building.ShouldUpgrade = false;
                StrategyDataManager.UpgradeBuildingLevel(building);
                i--;
               
            }
        }
    }

    //public bool IsUpgrading(BuildingDefinitionSO building)
    //{
    //    return BuildingsBeingUpgraded.Contains(building);
    //}

    public void RegisterBuildingController(BuildingController controller)
    {
        BuildingControllers.Add(controller);
    }


   
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


    /// <summary>
    /// Attempts to upgrade a building. If a an upgrade slot is missing, the UI will be shown
    /// </summary>
    /// <param name="building"></param>
    internal bool TryUpgradeBuilding(BuildingDefinitionSO building, bool isInstant = false)
    {
        if(building.BuildingType != BuildingType.Sub && BuildingsBeingUpgraded.Count >= BuildingGlobalConfig.GetUpgradeSlots())
        {
            StrategyUIManager.Instance.UpgradeSlotsView.Show(null);

            return false;
        }

       List<ResourceAmount> price = GetUpgradePrice(building);
        if (StrategyDataManager.CanAfford(price))
        {
            StrategyDataManager.Spend(price);
         
            if(building.BuildingType != BuildingType.Sub && !isInstant)
            {
                building.ShouldUpgrade = true;
                building.LastUpgradeTime = DateTime.UtcNow;
              
                BuildingsBeingUpgraded.Add(building);
                StrategyEvents.OnBuildingUpgradeStart?.Invoke(building);
              
               
            }
            else
            {
                StrategyDataManager.UpgradeBuildingLevel(building);
                
            }
           
            //if(building.BuildingType != BuildingType.Sub)
            //{
            //   SelectBuilding(building);
            //}

        }
        else
        {
            StrategyUIManager.Instance.InsufficentResourcesView.Show();
            return false;
        }
        return true;
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

    public List<ResourceAmount> GetUpgradePrice(BuildingDefinitionSO building)
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

    /// <summary>
    /// Returns how close a building is to being able to be upgraded. 100 means it can be upgraded. Different building types have different calculations
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    internal int GetMainBuildingUpgradePercentage(BuildingDefinitionSO building)
    {
        int requiredLevel = 0;
        int acquiredLevel = 0;
        // Master building percentage is based on the number of main buildings that have been upgraded
        if(building.BuildingType == BuildingType.Master)
        {
            foreach(var m in BuildingGlobalConfig.MainBuildings)
            {
                // If a building hasnt reached the minimum required level, it doesnt count towards the percentage


                if (m.MasterBuildingConstraint <= building.Level)
                {
                    if (m.MaxLevel < m.Level)
                    {

                        requiredLevel += m.MaxLevel;
                        acquiredLevel += m.MaxLevel;
                    }
                    else
                    {

                        requiredLevel += building.Level;
                        acquiredLevel += m.Level;
                    }

                    

                    
                }
            }

            int masterPercentage = (int)(((float)acquiredLevel / (float)requiredLevel) * 100f);

            return Mathf.Clamp(masterPercentage, 0,100);
        }

        // If a building doesnt have an upgrade constraint it is always 100%
        int numberOfSubBuildings = building.UpgradeConstraints.Count;
        if (numberOfSubBuildings == 0)
            return 100;
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

        int percentage = (int)(((float)acquiredLevel / (float)requiredLevel)*100f);
        return Mathf.Clamp(percentage, 0, 100);


    }

    internal bool CanUpgradeMainBuilding(BuildingDefinitionSO building)
    {
     

        return GetMainBuildingUpgradePercentage(building) >= 100;
    }

    internal bool CanUpgradeSubBuilding(BuildingDefinitionSO main, BuildingDefinitionSO sub)
    {
        int mainLevel = main.Level;
        int subLevel = sub.Level;

        int maxSubLevel = (mainLevel) * BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel;
        return maxSubLevel > subLevel;
    }
    /// <summary>
    /// Used to select a building. Opens UI and moves camera to specified building
    /// </summary>
    /// <param name="buildingDefinition"></param>
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
       
        if (buildingDefinition != null)
        {
            foreach(var cont in BuildingControllers)
            {
                if(cont.BuildingDefinition == buildingDefinition)
                {
                    cont.Select();
                    CurrentlySelectedBuildingController = cont;

                    StrategyMapCameraControls.Instance.SetCamera(cont.BuildingCamera);
                    StrategyMapCameraControls.Instance.SetSelected(cont);

                }
            }
          
            StrategyUIManager.Instance.BuildingStatusView.Show(buildingDefinition);
               
            
            
          
        }
        else
        {
            if (CurrentlySelectedBuildingController != null)
            {
                CurrentlySelectedBuildingController.UnSelect();
            }
            CurrentlySelectedBuildingController = null;
            StrategyMapCameraControls.Instance.SetDefaultCamera();
            StrategyUIManager.Instance.BuildingStatusView.Hide();
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

    public bool IsCurrentlyUpgrading(BuildingDefinitionSO building)
    {
        return BuildingsBeingUpgraded.Contains(building);
    }

    public bool IsUpgrading(BuildingDefinitionSO building)
    {
        ResourceAmountGroup group = BuildingGlobalConfig.GetUpgradeCost(building);
        DateTime lastUpgrade = building.LastUpgradeTime;
        int timeNeeded = BuildingGlobalConfig.GetUpgradeTime(building);
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
            return BuildingGlobalConfig.GetUpgradeTime(data);
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
      StrategyEvents.OnBuildingUpgradeCancelled?.Invoke(data);
        
    }

    internal void FinishUpgrade(BuildingDefinitionSO data)
    {
        BuildingsBeingUpgraded.Remove(data);
        StrategyDataManager.UpgradeBuildingLevel(data);
        data.ShouldUpgrade = false;
      
    }

    internal void SelectMasterBuilding()
    {
        SelectBuilding(BuildingGlobalConfig.MasterBuilding);
    }
    public string GetUpgradeTimer(BuildingDefinitionSO Data)
    {
        DateTime upgradeStarted = Data.LastUpgradeTime;
        DateTime upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(Data));
        TimeSpan timeSpan = upgradeFinishes - DateTime.UtcNow;
        return timeSpan.ToString(@"hh\:mm\:ss");
    }
    internal int GetGemsRequired(BuildingDefinitionSO data)
    {
        return ResourceManager.Instance.GetGemsRequired(GetUpgradeTimeSpan(data));
    }

    private TimeSpan GetUpgradeTimeSpan(BuildingDefinitionSO data)
    {
        if (BuildingsBeingUpgraded.Contains(data))
        {
            DateTime upgradeStarted = data.LastUpgradeTime;
            DateTime upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(data));
            return upgradeFinishes - DateTime.UtcNow;
        }
        else
        {
            return new TimeSpan(0, 0, GetUpgradeTime(data));
        }


    }

    internal bool InstantUpgrade(BuildingDefinitionSO data)
    {
        int gemsRequired = GetGemsRequired(data);
        if (StrategyDataManager.GetResource(ResourceType.Special) >= gemsRequired)
        {

            bool canUpgrade = TryUpgradeBuilding(data, true);
            if (canUpgrade)
            {
                StrategyDataManager.RemoveResource(ResourceType.Special, gemsRequired, "Instant_Build");
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            StrategyUIManager.Instance.InsufficentResourcesView.Show();
            StrategyUIManager.Instance.InsufficentResourcesView.Fill(new List<ResourceAmount>() { new ResourceAmount() { type = ResourceType.Special, amount = gemsRequired } });
            return false;
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
