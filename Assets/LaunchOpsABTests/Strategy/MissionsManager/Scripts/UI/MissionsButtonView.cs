using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionsButtonView : MonoBehaviour
{
    public GameObject MissionNotification;

    public TextMeshProUGUI CurrentChapterText;

    private void Start()
    {
        MissionsManager.Instance.OnMissionsUpdated += OnMissionsUpdated;
        BuildingManager.Instance.OnBuildingUpgraded += OnMissionsUpdated;
        OnMissionsUpdated();
    }

    private void OnMissionsUpdated(BuildingDefinitionSO sO)
    {
        OnMissionsUpdated();
    }

    private void OnMissionsUpdated()
    {
       CurrentChapterText.text = MissionsManager.Instance.CurrentSeasonData.seasonName;
        MissionNotification.SetActive(MissionsManager.Instance.MissionToClaim());
    }

    private void OnDestroy()
    {
        MissionsManager.Instance.OnMissionsUpdated -= OnMissionsUpdated;
        BuildingManager.Instance.OnBuildingUpgraded -= OnMissionsUpdated;
    }

    public void OnClick()
    {
         StrategyUIManager.Instance.MissionsView.Show();
        
    }

}
