using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingTimer : MonoBehaviour
{
    public Image UpgradeProgressFill;
    public TextMeshProUGUI UpgradeProgressText;
    public Image BuildingIcon;


    public void CancelBuild()
    {
        BuildingManager.Instance.CancelUpgrade(Data);
    }
    DateTime upgradeStarted;
    DateTime upgradeFinishes;
    BuildingDefinitionSO Data;
    internal void Fill(BuildingDefinitionSO data)
    {
        Data = data;
        upgradeStarted = data.LastUpgradeTime;
        upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(data));

    }
    private void Update()
    {
        TimeSpan timeSpan = upgradeFinishes -DateTime.UtcNow;
        if (timeSpan.TotalSeconds > 0)
        {
            int totalTime = BuildingManager.Instance.GetUpgradeTime(Data);
            UpgradeProgressFill.fillAmount = (float)(totalTime - timeSpan.TotalSeconds) / (float)BuildingManager.Instance.GetUpgradeTime(Data);
            UpgradeProgressText.text = timeSpan.ToString(@"hh\:mm\:ss");
        }
        else
        {
            UpgradeProgressFill.fillAmount = 0;
            UpgradeProgressText.text = "00:00:00";
        }
    }
}
