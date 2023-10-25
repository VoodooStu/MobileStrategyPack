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

        StrategyEvents.OnBuildingLevelChanged += OnMissionsUpdated;
        StrategyEvents.MissionUpdated += OnMissionsUpdated;
        OnMissionsUpdated();
    }


    private void OnMissionsUpdated(BuildingDefinitionSO missionSO)
    {
        OnMissionsUpdated();
    }
    private void OnMissionsUpdated(MissionSO missionSO)
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
        StrategyEvents.OnBuildingLevelChanged -= OnMissionsUpdated;
        StrategyEvents.MissionUpdated -= OnMissionsUpdated;
    }

    public void OnClick()
    {
         StrategyUIManager.Instance.MissionsView.Show();
        
    }

}
