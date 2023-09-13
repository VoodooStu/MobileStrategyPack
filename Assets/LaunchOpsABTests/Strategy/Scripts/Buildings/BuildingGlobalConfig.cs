using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuildingGlobalConfig", menuName = "LaunchOpsABTests/Strategy/BuildingGlobalConfig")]
public class BuildingGlobalConfig : ScriptableObject
{
    public int MaxBuildingLevel;
    public int UpgradeRestrictionLevel;


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

}
