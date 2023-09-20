using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionView : MonoBehaviour
{
    public TextMeshProUGUI missionName;
    public ResourceRewardView resourceRewardView;
    public Transform resourceRewardContainer;
    public GameObject missionCompleted;
    public GameObject missionClaimed;
    public MissionSO missionSO;
    private List<ResourceRewardView> rewardViews = new List<ResourceRewardView>();
    public GameObject GoToButton;
    public void ClaimMission()
    {
        MissionsManager.Instance.ClaimMission(missionSO);
    }

    internal void Fill(MissionSO mission)
    {
        missionSO = mission;    
        missionName.text = string.Format(mission.missionDescription,MissionsManager.Instance.GetMissionProgress(missionSO),missionSO.missionObjectiveValue);
        int progress = MissionsManager.Instance.GetMissionProgress(mission);
        int progressMax = mission.missionObjectiveValue;
     bool hasBeenClaimed = MissionsManager.Instance.HasBeenClaimed(missionSO);
        missionCompleted.SetActive(progress >= progressMax && !hasBeenClaimed);
        missionClaimed.SetActive(hasBeenClaimed);
        GoToButton.SetActive(progress < progressMax && !hasBeenClaimed && missionSO.buildingDefinitionSO!=null);
        foreach(var reward in missionSO.Rewards)
        {
            var view = Instantiate(resourceRewardView, resourceRewardContainer);
            rewardViews.Add(view);
            view.Fill(reward);  
        }

    }

    public void GoToBuilding()
    {
        BuildingManager.Instance.SelectBuilding(missionSO.buildingDefinitionSO);
        StrategyUIManager.Instance.MissionsView.Hide();
    }

  
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
