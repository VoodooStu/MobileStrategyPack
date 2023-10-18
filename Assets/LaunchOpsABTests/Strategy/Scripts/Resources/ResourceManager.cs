using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum ResourceType
{
    Time = -1,
    Soft = 0,
    Steel = 1,
    Special =2,
    Oil = 3,
    Food= 4,
}
[Serializable]
public class ResourceIcon
{
    public ResourceType Type;
    public Sprite Icon;
    public TMP_SpriteAsset SpriteAsset;
}

public class ResourceManager : MonoBehaviour
{
    private string ResourceGenKey = "RESOURCE_GEN";

    public SavedDateTime LastResourceGeneration;
    public ResourceConfigurationSO Configuration;
    private static ResourceManager _instance;
    public static ResourceManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
     
        _instance = this;
    }
    private void Start()
    {
        LastResourceGeneration = new SavedDateTime(ResourceGenKey, DateTime.UtcNow, true, null);

        CheckGeneration();
        CalculateOfflineEarnings();

    }

    private void CalculateOfflineEarnings()
    {
        Dictionary<ResourceType, float> resources = new Dictionary<ResourceType, float>();
        int numberOfSeconds = (int)(DateTime.UtcNow - LastResourceGeneration.Value).TotalSeconds;
        CalculateResources(ref resources, numberOfSeconds);
      
       

        float totalResources = 0;
        foreach(var r in resources)
        {
            totalResources += r.Value;
        }
        if (totalResources > 0)
        {
            StrategyUIManager.Instance.ResourceCollectView.Fill(resources, LastResourceGeneration);
            StrategyUIManager.Instance.ResourceCollectView.Show();
        }
        LastResourceGeneration.Value = DateTime.UtcNow;

    }

    public List<ResourceIcon> ResourceIcons = new List<ResourceIcon>();

    internal Sprite GetResourceIcon(ResourceType type)
    {
        var icon = ResourceIcons.Where(x => x.Type == type).FirstOrDefault();
        if(icon == null)
        {
            //Debug.LogError("No icon found for resource type " + type);
            return null;
        }
        return icon.Icon;
    }

    public void CheckGeneration()
    {
        DateTime lastGen = LastResourceGeneration.Value;
       
        TimeSpan timeSpan = DateTime.UtcNow - LastResourceGeneration.Value;
        if (timeSpan.TotalSeconds < 0)
        {
            LastResourceGeneration.Value = DateTime.UtcNow;
        }
        lastGen = LastResourceGeneration.Value;
    }

    internal float GetRate(BuildingDefinitionSO data, ResourceRate rate)
    {
        return data.Level * rate.Rate;
    }
    public float CurrentProductionPercentage;

    private void Update()
    {
        TimeSpan timeSpan = DateTime.UtcNow - LastResourceGeneration.Value;
        CurrentProductionPercentage = (float)timeSpan.TotalSeconds / Configuration.GenerationTime;
        if (timeSpan.TotalSeconds> Configuration.GenerationTime)
        {
            GenerateResources();
        }
    }
    private void CalculateResources( ref Dictionary<ResourceType, float> resources, int numberOfSeconds)
    {
        CalculateResources(BuildingManager.Instance.MainBuildings, ref resources, numberOfSeconds);
       
    }

    private void CalculateResources(List<BuildingDefinitionSO> buildings ,ref Dictionary<ResourceType, float> resources, int numberOfSeconds)
    {
        foreach (var building in buildings)
        {

            GenerateResources(building, ref resources, numberOfSeconds);
            if(building.UpgradeConstraints.Count > 0)
            {
                CalculateResources(building.UpgradeConstraints.Select(x => x.BuildingDefinition).ToList(), ref resources, numberOfSeconds);
            }
        }
        
    }
    private void GenerateResources()
    {
        Dictionary<ResourceType,float> resources = new Dictionary<ResourceType, float>();
        int numberOfSeconds = (int)(DateTime.UtcNow - LastResourceGeneration.Value).TotalSeconds;
        CalculateResources(ref resources, numberOfSeconds);

        RewardResources(resources);
    }

    private void RewardResources(Dictionary<ResourceType, float> resources)
    {
      

        LastResourceGeneration.Value = DateTime.UtcNow;
        foreach(var resource in resources)
        {
            StrategyDataManager.AddResource(resource.Key, resource.Value,"generation");
        }
    }

    private void GenerateResources(BuildingDefinitionSO building, ref Dictionary<ResourceType, float> resources, int numberOfSeconds)
    {
        numberOfSeconds = Mathf.Min (numberOfSeconds, Configuration.MaxGenerationTime);
        foreach (var rate in building.ResourceRates)
        {
            float amount = GetRate(building, rate);
            if(resources.ContainsKey(rate.Type))
            {
                resources[rate.Type] += amount *numberOfSeconds / Configuration.GenerationTime;
            }
            else
            {
                resources.Add(rate.Type, amount* numberOfSeconds / Configuration.GenerationTime);
            }
           
        }
    }

    internal int GetGemsRequired(TimeSpan timeSpan)
    {
        return (int)Mathf.Ceil((float)timeSpan.TotalSeconds / Configuration.SecondsToGems);
    }

    internal TMP_SpriteAsset GetResourceSpriteAsset(ResourceType type)
    {
        var icon = ResourceIcons.Where(x => x.Type == type).FirstOrDefault();
        if (icon == null)
        {
            Debug.LogError("No Sprite Asset found for resource type " + type);
            return null;
        }
        return icon.SpriteAsset;
    }
}
