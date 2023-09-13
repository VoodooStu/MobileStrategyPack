using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class BuildingController : MonoBehaviour
{
    public BuildingDefinitionSO BuildingDefinition;
    public Animator BuildingAnimator;
    public Cinemachine.CinemachineVirtualCamera BuildingCamera;
    public List<BuildingController> SubBuildings; 
    private void Start()
    {
        if(BuildingCamera!=null)
            BuildingCamera.Priority = -10;
        SetLevel(BuildingDefinition.Level, false);
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingLevelUp;
    }
    private void OnDestroy()
    {
        BuildingManager.Instance.OnBuildingUpgraded -= OnBuildingLevelUp;
    }
    private void OnBuildingLevelUp(BuildingDefinitionSO building)
    {
        if(building == BuildingDefinition)
        {
            SetLevel(building.Level, true);
        }
    }

    public void SetLevel(int level, bool shouldAnimate)
    {
        if (BuildingAnimator != null)
        {
            BuildingAnimator.SetInteger("Level", level);
            if (shouldAnimate)
            {
                BuildingAnimator.SetTrigger("Upgrade");

            }
            else
            {

                BuildingAnimator.SetTrigger("Set");

            }
        }

        foreach (var subBuilding in SubBuildings)
        {
            var data = BuildingDefinition.UpgradeConstraints.Find(x => x.BuildingDefinition == subBuilding.BuildingDefinition);
            if(data == null)
            {
                subBuilding.gameObject.SetActive(false);
            }
            else if(BuildingDefinition.Level>= data.StartLevel)
            {
                subBuilding.gameObject.SetActive(true);
            }
            else
            {
                subBuilding.gameObject.SetActive(false);

            }
        }
    }

    internal void Select()
    {
        BuildingAnimator.SetBool("Selected", true);
    }
     internal void UnSelect()
    {
        BuildingAnimator.SetBool("Selected", false);
    }
}
