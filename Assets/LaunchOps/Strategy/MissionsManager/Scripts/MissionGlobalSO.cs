using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "MissionGlobalSO", menuName = "Missions/MissionGlobalSO", order = 1)]
public class MissionGlobalSO : ScriptableObject
{
    public List<MissionSO> allMissions = new List<MissionSO>();
    public List<MissionSeason> seasons = new List<MissionSeason>(); 

    public int NumberOfDailyMissions = 3;

    internal MissionSO GetRandomDailyMission()
    {
        return allMissions.Find((u) => u.missionType == MissionType.Daily);
    }
}
[Serializable]
public class MissionSeason
{
    public string seasonName;
    public Sprite seasonImage;
    public List<MissionSO> missions = new List<MissionSO>();
}

