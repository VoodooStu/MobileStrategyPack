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
        foreach(var building in BuildingManager.Instance.MainBuildings)
        {
            var button = Instantiate(TestBuildingButtonPrefab, TestBuildingButtonParent);
            button.Fill(building,OnBuildingSelected);
        }
    }

    private void OnBuildingSelected(BuildingDefinitionSO building)
    {
        
            StrategyUIManager.Instance.BuildingStatusView.Show(building);
            
      
      

    }

}
