using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MissionType
{
    Daily,
    Permanent,
}


public enum MissionObjectiveType
{
    Score,
    Collect,
    Play,
    Custom,
    UpgradeBuilding,
    UpgradeSubBuilding,

}
/// <summary>
/// Extend this class to add rewards to the mission
/// </summary>
[CreateAssetMenu(fileName = "Mission", menuName = "Missions/Mission", order = 1)]
public partial class MissionSO : ScriptableObject
{
    public string missionName;
    public string missionDescription;
    public MissionType missionType;
    public MissionObjectiveType missionObjectiveType;
    public BuildingDefinitionSO buildingDefinitionSO;
    public int missionObjectiveValue;
    public List<ResourceAmount> Rewards = new List<ResourceAmount>();

    public bool CanClaim => MissionsManager.Instance.GetMissionProgress(this) >= missionObjectiveValue && !MissionsManager.Instance.HasBeenClaimed(this);

    public string ID => missionName+"_"+missionType.ToString()+"_"+missionObjectiveType.ToString()+"_"+ missionObjectiveValue;
}
