using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum ResourceType
{
    Soft = 0,
    Wood = 1,
    Special =2
}
[Serializable]
public class ResourceIcon
{
    public ResourceType Type;
    public Sprite Icon;
}

public class ResourceManager : MonoBehaviour
{
    private string ResourceGenKey = "RESOURCE_GEN";

    public SavedDateTime LastResourceGeneration;

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
      
        LastResourceGeneration.Value = DateTime.UtcNow;
        StrategyUIManager.Instance.ResourceCollectView.Fill(resources);
        StrategyUIManager.Instance.ResourceCollectView.Show();
    }

    public List<ResourceIcon> ResourceIcons = new List<ResourceIcon>();

    internal Sprite GetResourceIcon(ResourceType type)
    {
        var icon = ResourceIcons.Where(x => x.Type == type).FirstOrDefault();
        if(icon == null)
        {
            Debug.LogError("No icon found for resource type " + type);
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


    private void Update()
    {
        TimeSpan timeSpan = DateTime.UtcNow - LastResourceGeneration.Value;
       
        if(timeSpan.TotalSeconds>5)
        {
            GenerateResources();
        }
    }
    private void CalculateResources( ref Dictionary<ResourceType, float> resources, int numberOfSeconds)
    {
        CalculateResources(BuildingManager.Instance.Buildings, ref resources, numberOfSeconds);
       
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
        foreach (var rate in building.ResourceRates)
        {
            float amount = GetRate(building, rate);
            if(resources.ContainsKey(rate.Type))
            {
                resources[rate.Type] += amount *numberOfSeconds;
            }
            else
            {
                resources.Add(rate.Type, amount*numberOfSeconds);
            }
           
        }
    }
}
