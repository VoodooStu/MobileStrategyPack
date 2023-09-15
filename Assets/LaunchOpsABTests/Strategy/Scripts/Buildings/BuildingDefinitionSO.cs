using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingClass
{
    None
}

public enum BuildingType
{
    Master=0,
    Main=1,
    Sub=2,
    
}
[Serializable]
public class BuildingUpgradeConstraint
{
   
    public int StartLevel;
    public BuildingDefinitionSO BuildingDefinition;

}
[CreateAssetMenu(fileName = "BuildingDefinition", menuName = "LaunchOpsABTests/Strategy/BuildingDefinition")]
public class BuildingDefinitionSO : ScriptableObject
{
    public BuildingClass BuildingClass;
    public BuildingType BuildingType;
    public string BuildingName;
    public string BuildingDescription;
    public int MaxLevel => BuildingManager.Instance.GetMaxAllowedBuildingLevel(this);
    public Sprite Icon;
    public List<ResourceRate> ResourceRates;
    public int MasterBuildingConstraint;
    public List<BuildingUpgradeConstraint> UpgradeConstraints;
    public List<ResourceAmountGroup> UpgradePrices = new List<ResourceAmountGroup>();
    public int Level => StrategyDataManager.GetBuildingLevel(ID);
    public string ID => BuildingName +"_"+BuildingClass.ToString() + "_" + BuildingType.ToString();

    public bool IsUnlocked => Level > 0;

    internal int GetLevelRestriction(BuildingDefinitionSO sub)
    {
        return UpgradeConstraints.Find(x => x.BuildingDefinition == sub).StartLevel;
    }

    public DateTime LastUpgradeTime
    {
        get => StrategyDataManager.GetBuildingLastUpgradeTime(ID); set => StrategyDataManager.SetBuildingLastUpgradeTime(ID, value);
    }

    public bool ShouldUpgrade
    {
        get => StrategyDataManager.GetBuildingShouldUpgrade(ID); set => StrategyDataManager.SetBuildingShouldUpgrade(ID, value);
    }

}
