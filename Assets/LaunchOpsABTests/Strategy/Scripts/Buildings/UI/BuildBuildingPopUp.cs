using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBuildingPopUp : MonoBehaviour
{
    public ResourceView ResourceView;
    private List<ResourceView> ResourceViews = new List<ResourceView>();
    
    public Action<BuildingDefinitionSO> OnClick;
    private BuildingDefinitionSO Data;
    public void Fill(BuildingDefinitionSO _data, Action<BuildingDefinitionSO> _onClick)
    {
        Data = _data;
        OnClick = _onClick;
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


    private void Awake()
    {
        Hide();
    }
    public void OnClickButton()
    {
        OnClick?.Invoke(Data);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
