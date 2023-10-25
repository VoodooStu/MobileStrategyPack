using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ResourceConfigurationSO", menuName = "ScriptableObjects/ResourceConfigurationSO", order = 1)]
public class ResourceConfigurationSO : ScriptableObject
{
    [Header("Upgrade Configurations")]
    [Tooltip("Defines the max number of extra upgrade slots that can be purchased")]
    public int MaxAllowedExtraBuildingSlots = 1;
    [Tooltip("Example: If a building has 1 minute (60 seconds) until it upgrades the gem cost is equal to 60 divided by the SecondsToGems paramater.")]

    public int SecondsToGems = 5;
    [Header("Resource Generation")]
    [Tooltip("How often that buildings will generate resources in game.")]

    public float GenerationTime = 5f;
    [Tooltip("The maximum amount of seconds that a user can generate offline.")]

    public int MaxGenerationTime = 50;
  
}
