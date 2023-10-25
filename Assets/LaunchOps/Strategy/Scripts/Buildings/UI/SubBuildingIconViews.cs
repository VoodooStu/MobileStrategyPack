using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBuildingIconViews : MonoBehaviour
{
    public SubBuildingView SubBuildingPrefab;
    public Transform SubBuildingContainer;

    private List<SubBuildingView> SubBuildingViews = new List<SubBuildingView>();
    BuildingDefinitionSO Data;
    BuildingDefinitionSO SubBuildingData;
    Action<BuildingDefinitionSO> OnClick;
    public void ClearCurrentIcons()
    {
        while (SubBuildingViews.Count > 0)
        {
            Destroy(SubBuildingViews[0].gameObject);
            SubBuildingViews.RemoveAt(0);
        }
    }
    public void Fill(BuildingDefinitionSO data, Action<BuildingDefinitionSO> onClick)
    {
        Data = data;
        ClearCurrentIcons();
        OnClick = onClick;

        if (data.UpgradeConstraints.Count >0)
        {
            SubBuildingData = data.UpgradeConstraints[0].BuildingDefinition;
            foreach (var building in Data.UpgradeConstraints)
            {
                if (building.StartLevel <= Data.Level)
                {
                    SubBuildingView view = Instantiate(SubBuildingPrefab, SubBuildingContainer);
                    SubBuildingViews.Add(view);
                    view.Fill(building, SelectSubBuilding, building.BuildingDefinition == SubBuildingData);

                }

            }
        }
    }

    public void SubBuildingChanged(BuildingDefinitionSO data)
    {
        SubBuildingData = data;
        foreach (var view in SubBuildingViews)
        {
            view.SetSelected(SubBuildingData);
        }
    }
    public void SelectSubBuilding(BuildingDefinitionSO data)
    {
        if (data == null)
        {
            return;
        }
        if (data == SubBuildingData)
        {
            return;
        }
        SubBuildingData = data;
        foreach(var view in SubBuildingViews)
        {
            view.SetSelected( SubBuildingData);
        }
        OnClick?.Invoke(data);
    }

}
