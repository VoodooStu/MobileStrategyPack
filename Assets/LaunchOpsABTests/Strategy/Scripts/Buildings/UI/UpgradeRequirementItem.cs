using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class UpgradeRequirementItem : MonoBehaviour
{
    public TextMeshProUGUI RequirementText;
    public Image RequirementIcon;

    private BuildingUpgradeConstraint _constraint;
    public GameObject GoToButton;
    public GameObject RequirementReached;
    public GameObject RequirementNotReached;
    public Image BackgroundImage;
    public void Fill(BuildingUpgradeConstraint constraint)
    {
        RequirementReached.SetActive(false);
        RequirementNotReached.SetActive(false);
        GoToButton.SetActive(false);
        _constraint = constraint;
        RequirementIcon.sprite = constraint.BuildingDefinition.Icon;
        RequirementText.text = constraint.BuildingDefinition.DisplayName + " Level (" +constraint.BuildingDefinition.Level+ "/" + constraint.StartLevel+")";
        RequirementNotReached.SetActive(false);
        if (constraint.BuildingDefinition.Level < constraint.StartLevel)
        {
            if(constraint.BuildingDefinition.BuildingType != BuildingType.Sub)
            {
                GoToButton.SetActive(true);
            }
            else
            {
                RequirementNotReached.SetActive(true);
            }
            GoToButton.SetActive(constraint.BuildingDefinition.BuildingType != BuildingType.Sub);
           
        }
        else
        {
          
            RequirementReached.SetActive(true);
        }
    }

    public void OnGoToClick()
    {
        if (_constraint != null)
        {
            BuildingManager.Instance.SelectBuilding(null);
            BuildingManager.Instance.SelectBuilding(_constraint.BuildingDefinition);
        }
    }


    public void Fill(ResourceAmount requiredResource)
    {
        float balance = StrategyDataManager.GetResource(requiredResource.type);
        RequirementReached.SetActive(balance >= requiredResource.amount);
        RequirementIcon.sprite = ResourceManager.Instance.GetResourceIcon(requiredResource.type);
        GoToButton.SetActive(false);
        RequirementText.text = balance.ToReadableString()+"/" +requiredResource.amount.ToReadableString();
        RequirementNotReached.SetActive(balance <requiredResource.amount);
    }

    internal void SetColor(Color color)
    {
        BackgroundImage.color = color;
    }
}
