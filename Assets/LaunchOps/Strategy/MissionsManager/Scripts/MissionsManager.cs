using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[Serializable]
public class DailyMissionSaveData
{
    public List<string> CurrentDailyMissions = new List<string>();
    public DateTime lastDailyMissionRefreshTime;
}




public class MissionsManager : MonoBehaviour
{
    public static MissionsManager Instance { get; private set; }

    public SavedInt CurrentSeason;


    public MissionSeason CurrentSeasonData
    {
        get
        {
            return missionGlobalSO.seasons[Mathf.Clamp(CurrentSeason.Value, 0, missionGlobalSO.seasons.Count-1)];
        }
    }

    public List<MissionSO> CurrentSeasonMissions
    {
        get
        {
            return missionGlobalSO.seasons[Mathf.Clamp(CurrentSeason.Value,0, missionGlobalSO.seasons.Count-1)].missions;
        }
    }

    internal bool MissionToClaim()
    {
        var missions = CurrentSeasonMissions;
        foreach (var item in missions)
        {
            if (item.CanClaim)
            {
                return true;
            }
        }
        return false;
    }

    public MissionGlobalSO missionGlobalSO;


    [HideInInspector]
    public List<MissionSO> currentDailyMissions = new List<MissionSO>();

    private DailyMissionSaveData dailyMissionSaveData = null;
    
    private const string DAILY_MISSIONS_KEY = "DAILY_MISSIONS_KEY";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            return;
        }
        InitialiseMissions();
        CurrentSeason = new SavedInt("CurrentSeason", 0,true,null);

    }

    private void InitialiseMissions()
    {
        
    }
   

    internal int GetMissionProgress(MissionSO mission)
    {
        if(mission.missionObjectiveType == MissionObjectiveType.UpgradeBuilding || mission.missionObjectiveType == MissionObjectiveType.UpgradeSubBuilding)
        {
            return mission.buildingDefinitionSO.Level;
        }


        return PlayerPrefs.GetInt(mission.ID, 0);
    }

    internal void ClaimMission(MissionSO mission)
    {
        PlayerPrefs.SetInt("CLAIMED_"+ mission.ID, 1);
        PlayerPrefs.Save();
        foreach (var reward  in mission.Rewards)
        {
            StrategyDataManager.AddResource(reward.type, reward.amount,"Mission_Claim");
        }
     
        if(!IsLastSeason() &&CurrentSeasonComplete())
        {
            CurrentSeason.Value = Mathf.Clamp(CurrentSeason.Value+1, 0, missionGlobalSO.seasons.Count - 1);
            StrategyUIManager.Instance.SeasonCompletePopUp.Show();
            StrategyUIManager.Instance.SeasonCompletePopUp.Fill(CurrentSeasonData);
            StrategyUIManager.Instance.MissionsView.Hide();
        }
        StrategyEvents.MissionUpdated?.Invoke(mission);
        

    }

    public bool IsLastSeason()
    {
        if (CurrentSeason.Value +1 == missionGlobalSO.seasons.Count)
        {
            return true;
        }
        return false;
    }

    private bool CurrentSeasonComplete()
    {
        foreach(var mission in CurrentSeasonMissions)
        {
            if (!HasBeenClaimed(mission))
            {
                return false;
            }

        }
        return true;
    }

    internal bool HasBeenClaimed(MissionSO mission)
    {
        return PlayerPrefs.HasKey("CLAIMED_"+mission.ID);
    }

    internal void ClaimAll()
    {
        foreach(var mission in CurrentSeasonMissions)
        {
            if(mission.CanClaim)
            {
                ClaimMission(mission);
            }
        }
    }
}
