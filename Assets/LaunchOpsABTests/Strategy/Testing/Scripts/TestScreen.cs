using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreen : MonoBehaviour
{
    public TestBuildingButton TestBuildingButtonPrefab;
    public Transform TestBuildingButtonParent;

    private void Start()
    {
        Init();
    }
    

    public void Init()
    {
        foreach(var building in BuildingManager.Instance.Buildings)
        {
            var button = Instantiate(TestBuildingButtonPrefab, TestBuildingButtonParent);
            button.Fill(building,OnBuildingSelected);
        }
    }

    private void OnBuildingSelected(BuildingDefinitionSO building)
    {
        if (building.IsUnlocked)
        {
            StrategyUIManager.Instance.BuildingStatusView.Fill(building);
            StrategyUIManager.Instance.BuildingStatusView.Show();
        }
        else
        {
            StrategyUIManager.Instance.BuildBuildingPopUp.Fill(building,BuildingManager.Instance.TryUpgradeBuilding);
            StrategyUIManager.Instance.BuildBuildingPopUp.Show();
        }
      

    }

}
