using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBuildingPopUp : MonoBehaviour
{
    public ResourceView ResourceView;
    private List<ResourceView> ResourceViews = new List<ResourceView>();
    
    private BuildingDefinitionSO Data;
    public void Fill(BuildingDefinitionSO _data)
    {
        Data = _data;
        
        while(ResourceViews.Count >0)
        {
            Destroy(ResourceViews[0].gameObject);
            ResourceViews.RemoveAt(0);
        }
        foreach (var resource in BuildingManager.Instance.GetUpgradePrice(_data))
        {
            var resourceView = Instantiate(ResourceView, ResourceView.transform.parent);
            resourceView.Fill(resource);
            ResourceViews.Add(resourceView);
            resourceView.gameObject.SetActive(true);
        }

    }


    private void Start()
    {
        this.gameObject.SetActive(false);
    }
    public void OnClickButton()
    {
       
        Hide();
        BuildingManager.Instance.TryUpgradeBuilding(Data);
       
       
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {

        BuildingManager.Instance.SelectBuilding(null);
        this.gameObject.SetActive(false);
    }
}
