using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ResourceConfigurationSO", menuName = "ScriptableObjects/ResourceConfigurationSO", order = 1)]
public class ResourceConfigurationSO : ScriptableObject
{
    public int SecondsToGems = 5;
    public float GenerationTime = 5f;
    public int MaxGenerationTime = 50;
    public int MaxOfflineResourceAmount = 1000;
    public int MaxAllowedExtraBuildingSlots = 1;
}
