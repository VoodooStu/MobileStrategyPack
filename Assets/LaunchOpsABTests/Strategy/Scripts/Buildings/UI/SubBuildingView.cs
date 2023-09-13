using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubBuildingView : MonoBehaviour
{
    public Image BuildingIcon;
   
    public TextMeshProUGUI Level;
    private BuildingUpgradeConstraint Data;

    private Action<BuildingDefinitionSO> onClick;

    public GameObject SelectedIndicator;
    internal void Fill(BuildingUpgradeConstraint building, Action<BuildingDefinitionSO> _onClick, bool selected)
    {
        SelectedIndicator.SetActive(selected);
        Data = building;
        BuildingIcon.sprite = building.BuildingDefinition.Icon;
        Level.text = building.BuildingDefinition.Level.ToString();
        onClick = _onClick;
    }

    public void OnClick()
    {
        onClick?.Invoke(Data.BuildingDefinition);
    }

    internal void SetSelected(BuildingDefinitionSO subBuildingData)
    {
        SelectedIndicator.SetActive(Data.BuildingDefinition == subBuildingData);
    }
}
