using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public static class StrategyDataManager
{
    public static Dictionary<ResourceType, SavedFloat> Resources = new Dictionary<ResourceType, SavedFloat>();
    public static Action OnResourceChanged = null;
    public static void InitialiseResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            Resources.Add(type, new SavedFloat("VD_Cur_" + type.ToString(), 0, true, () => {
                OnResourceChanged?.Invoke();
            
            }));
        }
    }

    public static void AddResource(ResourceType type, float amount, string source, AnalyticsHelper.CurrencyTransactionType transactionType = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        Debug.Log(string.Format("Adding Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value +=amount;
        // TODO Add analytics
    }

    public static void SetResource(ResourceType type, float amount)
    {
        Debug.Log(string.Format("Setting Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value =amount;
        // TODO Add analytics
    }

    public static void RemoveResource(ResourceType type, float amount, string source, AnalyticsHelper.CurrencyTransactionType transactionType = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        Debug.Log(string.Format("removing Resource of type {0}: {1}", type.ToString(), amount));
        Resources[type].Value -=amount;
        // TODO Add analytics
    }

    public static float GetResource(ResourceType type)
    {
        return Resources[type].Value;
    }


    public static Dictionary<string, SavedInt> BuildingLevels = new Dictionary<string, SavedInt>();
    public static Action OnBuildingLevelChanged = null;
    public static void RegisterBuilding(BuildingDefinitionSO definition)
    {
        if (BuildingLevels.ContainsKey(definition.ID))
        {
            Debug.LogError("Building Already Present in Dictionary: " + definition.ID);
            return;
        }
        BuildingLevels.Add(definition.ID, new SavedInt("VD_Building_" + definition.ID, 0, true, () =>
        {
            OnBuildingLevelChanged?.Invoke();
        }));
    }

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

    public static void UpgradeBuildingLevel(string id)
    {
        BuildingLevels[id].Value++ ;
    }

    internal static bool CanAfford(List<ResourceAmount> price)
    {
        bool canAfford = true;
        foreach(var p in price)
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

