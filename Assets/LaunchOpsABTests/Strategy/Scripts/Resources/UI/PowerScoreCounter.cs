using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerScoreCounter : MonoBehaviour
{
    public TextMeshProUGUI PowerScoreText;

    // Start is called before the first frame update
    void Start()
    {
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingUpgraded;
        OnBuildingUpgraded(null);
    }
    private void OnDestroy()
    {
        BuildingManager.Instance.OnBuildingUpgraded -= OnBuildingUpgraded;
    }

    private void OnBuildingUpgraded(BuildingDefinitionSO sO)
    {
        PowerScoreText.text = BuildingManager.Instance.BuildingGlobalConfig.GetCurrentPowerLevel().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
