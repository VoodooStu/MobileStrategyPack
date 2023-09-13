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
    private void Start()
    {
        BuildingCamera.Priority = -10;
        SetLevel(BuildingDefinition.Level, false);
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingLevelUp;
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
