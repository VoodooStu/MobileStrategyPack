using Cinemachine;
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
    public BuildingNameTagView NameTag;
    private void Awake()
    {
        if(BuildingDefinition == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        if(NameTag!=null)
            NameTag.enabled = false;
    }
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
            BuildingName.text = BuildingDefinition.DisplayName;
        }
        SetLevel(BuildingDefinition.Level, false);
       
        StrategyEvents.OnBuildingLevelChanged += OnBuildingLevelUp;
        StrategyEvents.OnBuildingUpgradeStart += OnBuildingBeginsUpgrade;
       
        CheckMasterBuildingConstraint();
        CheckIfUpgrading();
       
    }

    private void OnBuildingLevelUp()
    {
        CheckIfUpgrading();
    }

    private void OnBuildingBeginsUpgrade(BuildingDefinitionSO building)
    {
        if(building == BuildingDefinition)
        {
              CheckIfUpgrading();
        }
    }

    void CheckIfUpgrading()
    {
        if (BuildingAnimator != null)
        {
            BuildingAnimator.SetBool("Upgrading",BuildingManager.Instance.IsCurrentlyUpgrading(BuildingDefinition));
        }
    }

    private void OnDestroy()
    {

        StrategyEvents.OnBuildingLevelChanged += OnBuildingLevelUp;
        StrategyEvents.OnBuildingUpgradeStart += OnBuildingBeginsUpgrade;

    }

    public void CheckMasterBuildingConstraint()
    {

        if (BuildingAnimator != null)
        {
            BuildingAnimator.SetBool("CanBeConstructed", BuildingDefinition.MasterBuildingConstraint <= BuildingManager.Instance.MasterBuilding.Level);
            BuildingAnimator.SetBool("Constructed", BuildingDefinition.Level > 0);
        }
        else
        {
            if (BuildingDefinition.BuildingType == BuildingType.Sub)
            {
                this.gameObject.SetActive(BuildingDefinition.Level > 0);
                
            }
            else
            {
                this.gameObject.SetActive(BuildingDefinition.MasterBuildingConstraint <= BuildingManager.Instance.MasterBuilding.Level);

            }
        }
       
        if (NameTag != null && !NameTag.enabled)
        {
            NameTag.enabled = BuildingDefinition.MasterBuildingConstraint <= BuildingManager.Instance.MasterBuilding.Level;
            NameTag.gameObject.SetActive(BuildingDefinition.MasterBuildingConstraint <= BuildingManager.Instance.MasterBuilding.Level);

        }


    }

    private void OnBuildingLevelUp(BuildingDefinitionSO building)
    {
        CheckMasterBuildingConstraint();

        if (building == BuildingDefinition)
        {
            SetLevel(building.Level, true);
        }
        CheckIfUpgrading();
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
        if(BuildingAnimator!=null)
            BuildingAnimator.SetBool("Selected", true);
    }
     internal void UnSelect()
    {
        if (BuildingAnimator != null)
            BuildingAnimator.SetBool("Selected", false);
    }
}