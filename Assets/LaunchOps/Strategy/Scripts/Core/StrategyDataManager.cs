using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
public static class StrategyDataManager
{
    public static Dictionary<ResourceType, SavedFloat> Resources = new Dictionary<ResourceType, SavedFloat>();
    public static Action OnResourceChanged = null;

    /// <summary>
    /// This is the amount of extra building slots the player has purchased
    /// </summary>
    public static SavedInt ExtraBuildingSlots = new SavedInt("VD_BuildingSlots", 0, true, () =>
    {
        OnResourceChanged?.Invoke();
    });
    /// <summary>
    /// sets up a save state for every defined resources
    /// </summary>
    public static void InitialiseResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            Resources.Add(type, new SavedFloat("VD_Cur_" + type.ToString(), 0, true, () => {
                OnResourceChanged?.Invoke();

            }));
        }
        if (StrategyManager.Instance.DebugResources)
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                Resources[type].Value = 100000000;
            }
        }

    }
    /// <summary>
    /// TODO add your own resource save logic here
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <param name="source"></param>
    /// <param name="transactionType"></param>
    public static void AddResource(ResourceType type, float amount, string source, AnalyticsHelper.CurrencyTransactionType transactionType = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        Debug.Log(string.Format("Adding Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value += amount;
        AnalyticsHelper.SimpleTrackCurrency(amount, source, type.ToString());
       
    }
    /// <summary>
    /// TODO add your own resource save logic here
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public static void SetResource(ResourceType type, float amount)
    {
        Debug.Log(string.Format("Setting Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value = amount;
      
    }
    /// <summary>
    /// TODO add your own resource save logic here
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <param name="source"></param>
    /// <param name="transactionType"></param>
    public static void RemoveResource(ResourceType type, float amount, string source, AnalyticsHelper.CurrencyTransactionType transactionType = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        Debug.Log(string.Format("removing Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value -= amount;
        AnalyticsHelper.SimpleTrackCurrency(amount, source, type.ToString());
    }


    public static float GetResource(ResourceType type)
    {
        return Resources[type].Value;
    }


    public static Dictionary<string, SavedInt> BuildingLevels = new Dictionary<string, SavedInt>();
    public static Dictionary<string, SavedBool> BuildingShouldUpgrade = new Dictionary<string, SavedBool>();
    public static Dictionary<string, SavedDateTime> BuildingUpgradeTime = new Dictionary<string, SavedDateTime>();
    /// <summary>
    /// This function sets all the save info for the buildings
    /// </summary>
    /// <param name="definition"></param>
    public static void RegisterBuilding(BuildingDefinitionSO definition)
    {
        if (BuildingLevels.ContainsKey(definition.ID))
        {
            Debug.LogError("Building Already Present in Dictionary: " + definition.ID);
            return;
        }
        BuildingLevels.Add(definition.ID, new SavedInt("VD_Building_" + definition.ID, 0, true, () =>
        {
            StrategyEvents.OnBuildingLevelChanged?.Invoke(definition);
        }));
        BuildingShouldUpgrade.Add(definition.ID, new SavedBool("VD_Building_" + definition.ID + "_ShouldUpgrade", false, true, () =>
        {

        }));
        BuildingUpgradeTime.Add(definition.ID, new SavedDateTime("VD_Building_" + definition.ID + "_UpgradeTime", DateTime.UtcNow, true, () =>
        {

        }));
    }
    #region Building Data
    public static void SetBuildingLevel(string id, int level)
    {
        BuildingLevels[id].Value = level;
    }
    public static int GetBuildingLevel(string id)
    {
        return BuildingLevels[id].Value;
    }
    public static int GetBuildingLevel(BuildingDefinitionSO definition)
    {
        return BuildingLevels[definition.ID].Value;
    }

    public static void UpgradeBuildingLevel(BuildingDefinitionSO building)
    {
        BuildingLevels[building.ID].Value++;
        StrategyEvents.OnBuildingLevelChanged?.Invoke(building);
    }
    internal static DateTime GetBuildingLastUpgradeTime(string iD)
    {
        return BuildingUpgradeTime[iD].Value;
    }

    internal static void SetBuildingLastUpgradeTime(string iD, DateTime value)
    {
        BuildingUpgradeTime[iD].Value = value;
    }

    internal static bool GetBuildingShouldUpgrade(string iD)
    {
        return BuildingShouldUpgrade[iD].Value;
    }

    internal static void SetBuildingShouldUpgrade(string iD, bool value)
    {
        BuildingShouldUpgrade[iD].Value = value;
    }
    #endregion
    internal static bool CanAfford(List<ResourceAmount> price)
    {
        bool canAfford = true;
        foreach (var p in price)
        {
            float amount = GetResource(p.type);
            if (amount < p.amount)
            {
                canAfford = false;
                break;
            }
        }
        return canAfford;
    }

    internal static void Spend(List<ResourceAmount> price)
    {
        foreach (var p in price)
        {
            RemoveResource(p.type, p.amount, "Building Upgrade");

        }
    }

    
}



