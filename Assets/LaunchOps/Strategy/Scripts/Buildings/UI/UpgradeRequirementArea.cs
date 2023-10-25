using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Privacy;

public class UpgradeRequirementArea : MonoBehaviour
{
    public UpgradeRequirementItem upgradeRequirementItem;
    private List<UpgradeRequirementItem> upgradeRequirementItems = new List<UpgradeRequirementItem>();
    public Transform content;
    public Color FirstColor;
    public Color SecondColor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Fill(BuildingDefinitionSO building)
    {
        while (upgradeRequirementItems.Count > 0)
        {
            Destroy(upgradeRequirementItems[0].gameObject);
            upgradeRequirementItems.RemoveAt(0);
        }
        if(building.BuildingType == BuildingType.Main)
        {
            var requirement = Instantiate(upgradeRequirementItem, content);
            requirement.Fill(new BuildingUpgradeConstraint()
            {
                StartLevel = Mathf.Max(building.MasterBuildingConstraint,building.Level),
                BuildingDefinition = BuildingManager.Instance.MasterBuilding
            });
            upgradeRequirementItems.Add(requirement);

            foreach(var subConstraint in building.UpgradeConstraints)
            {
                if (subConstraint.StartLevel > building.Level)
                    continue;
                var requirementItem = Instantiate(upgradeRequirementItem, content);
                requirementItem.Fill(new BuildingUpgradeConstraint()
                {
                    StartLevel = building.Level * BuildingManager.Instance.BuildingGlobalConfig.UpgradeRestrictionLevel,
                    BuildingDefinition = subConstraint.BuildingDefinition
                });
                upgradeRequirementItems.Add(requirementItem);
            }

        }
        if (building.BuildingType == BuildingType.Master)
        {

            foreach (var m in BuildingManager.Instance.BuildingGlobalConfig.MainBuildings)
            {
                if (m.MasterBuildingConstraint <= building.Level && !m.IsMaxLevel)
                {


                    var requirement = Instantiate(upgradeRequirementItem, content);
                    requirement.Fill(new BuildingUpgradeConstraint()
                    {
                        StartLevel = building.Level,
                        BuildingDefinition = m
                    });
                    upgradeRequirementItems.Add(requirement);

                }
            }
        }
        var prices = BuildingManager.Instance.GetUpgradePrice(building);
        foreach(var price in prices)
        {
            var requirement = Instantiate(upgradeRequirementItem, content);
            requirement.Fill(price);
            upgradeRequirementItems.Add(requirement);
        }

        for(int i = 0;i< upgradeRequirementItems.Count; i++)
        {
            upgradeRequirementItems[i].SetColor(i % 2 == 0 ? FirstColor : SecondColor);
        }


    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
