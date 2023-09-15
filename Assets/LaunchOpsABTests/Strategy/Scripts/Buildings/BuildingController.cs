using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class BuildingController : MonoBehaviour
{
    public BuildingDefinitionSO BuildingDefinition;
    public Animator BuildingAnimator;
    public Cinemachine.CinemachineVirtualCamera BuildingCamera;
    public List<BuildingController> SubBuildings; 
    public TextMeshProUGUI BuildingName;
    private void Start()
    {
        BuildingManager.Instance.RegisterBuildingController(this);
        if (BuildingCamera != null)
        {
            BuildingCamera.Priority = -10;
            BuildingCamera.gameObject.SetActive(false);
        }
        if (BuildingName != null)
        {
            BuildingName.text = BuildingDefinition.BuildingName;
        }
        SetLevel(BuildingDefinition.Level, false);
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingLevelUp;
        CheckMasterBuildingConstraint();
    }
    private void OnDestroy()
    {
        if(BuildingManager.Instance!=null)
            BuildingManager.Instance.OnBuildingUpgraded -= OnBuildingLevelUp;
    }

    public void CheckMasterBuildingConstraint()
    {
        if (BuildingDefinition.BuildingType == BuildingType.Main)
        {
            this.gameObject.SetActive(BuildingDefinition.MasterBuildingConstraint <= BuildingManager.Instance.MasterBuilding.Level);

        }
    }

    private void OnBuildingLevelUp(BuildingDefinitionSO building)
    {
        CheckMasterBuildingConstraint();

        if (building == BuildingDefinition)
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
