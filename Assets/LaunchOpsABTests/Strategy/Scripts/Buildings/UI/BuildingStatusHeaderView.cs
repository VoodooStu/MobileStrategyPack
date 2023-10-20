using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingHeaderState
{
    Upgrading,
    UpgradeAvailable,
    Maxed,
    UpgradeMaster,
    UpgradeUnAvailable,
    BeginUpgrade,
    Build
}

public class BuildingStatusHeaderView : MonoBehaviour
{
    public TextMeshProUGUI BuildingTitleText;
    [Header("Building Status Indicators")]
    public TextMeshProUGUI MainBuildingUpgradeProgressText;
    public Image MainBuildingUpgradeProgressFill;
    public GameObject UpgradeNotAvailableImage;
    public GameObject UpgradeAvailableImage;
    public GameObject MaxedImage;
    public GameObject UpgradeMasterIcon;
    public GameObject UpgradingImage;
    public GameObject BackButton;
    public GameObject BuildButton;
    
    BuildingDefinitionSO Data;
    public void Fill(BuildingDefinitionSO data)
    {
        Data = data;
        int UpgradeProgress = BuildingManager.Instance.GetMainBuildingUpgradePercentage(Data);
        MainBuildingUpgradeProgressText.text = UpgradeProgress + "%";
        MainBuildingUpgradeProgressFill.fillAmount = (float)UpgradeProgress / 100f;
        BuildingTitleText.text = data.DisplayName + " Lv." + data.Level;
    }

    public void GoToMainBuilding()
    {
        BuildingManager.Instance.SelectMasterBuilding();
    }

    public void SetState(BuildingHeaderState state)
    {
        UpgradingImage.SetActive(false);
        UpgradeAvailableImage.SetActive(false);
        MaxedImage.SetActive(false);
        UpgradeMasterIcon.SetActive(false);
        UpgradeNotAvailableImage.SetActive(false);
        BackButton.SetActive(false);
       BuildButton.SetActive(false);
        switch (state)
        {
            case BuildingHeaderState.Upgrading:
                UpgradingImage.SetActive(true);
                
                break;
            case BuildingHeaderState.UpgradeAvailable:
                
                UpgradeAvailableImage.SetActive(true);
               
                break;
            case BuildingHeaderState.Maxed:
              
                MaxedImage.SetActive(true);
               
                break;
            case BuildingHeaderState.UpgradeMaster:
               
                UpgradeMasterIcon.SetActive(true);
               
                break;
            case BuildingHeaderState.UpgradeUnAvailable:
               
                UpgradeNotAvailableImage.SetActive(true);
                break;
            case BuildingHeaderState.BeginUpgrade:

                BackButton.SetActive(true);
                break;
            case BuildingHeaderState.Build:

                BuildButton.SetActive(true);
                break;

            default:
                break;
        }
    }

}
