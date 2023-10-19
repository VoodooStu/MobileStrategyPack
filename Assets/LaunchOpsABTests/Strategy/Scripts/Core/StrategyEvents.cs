using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyEvents
{
    /// <summary>
    /// Building Events
    /// </summary>
    public static Action<BuildingDefinitionSO> OnBuildingUpgradeStart;
    public static Action<BuildingDefinitionSO> OnBuildingLevelChanged;
    public static Action<BuildingDefinitionSO> OnBuildingSelected;
    public static Action<BuildingDefinitionSO> OnBuildingUpgradeCancelled;

    /// <summary>
    /// Resource Events
    /// </summary>
    public static Action<ResourceType> OnResourceChanged;


    public static Action<MissionSO> MissionUpdated;

   
}
