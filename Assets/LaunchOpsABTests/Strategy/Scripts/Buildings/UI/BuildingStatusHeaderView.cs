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
    UpgradeUnAvailable
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
        switch (state)
        {
            case BuildingHeaderState.Upgrading:
                UpgradingImage.SetActive(true);
                UpgradeAvailableImage.SetActive(false);
                MaxedImage.SetActive(false);
                UpgradeMasterIcon.SetActive(false);
                UpgradeNotAvailableImage.SetActive(false);
                break;
            case BuildingHeaderState.UpgradeAvailable:
                UpgradingImage.SetActive(false);
                UpgradeAvailableImage.SetActive(true);
                MaxedImage.SetActive(false);
                UpgradeMasterIcon.SetActive(false);
                UpgradeNotAvailableImage.SetActive(false);
                break;
            case BuildingHeaderState.Maxed:
                UpgradingImage.SetActive(false);
                UpgradeAvailableImage.SetActive(false);
                MaxedImage.SetActive(true);
                UpgradeMasterIcon.SetActive(false);
                UpgradeNotAvailableImage.SetActive(false);
                break;
            case BuildingHeaderState.UpgradeMaster:
                UpgradingImage.SetActive(false);
                UpgradeAvailableImage.SetActive(false);
                MaxedImage.SetActive(false);
                UpgradeMasterIcon.SetActive(true);
                UpgradeNotAvailableImage.SetActive(false);
                break;
            case BuildingHeaderState.UpgradeUnAvailable:
                UpgradingImage.SetActive(false);
                UpgradeAvailableImage.SetActive(false);
                MaxedImage.SetActive(false);
                UpgradeMasterIcon.SetActive(false);
                UpgradeNotAvailableImage.SetActive(true);
                break;
            default:
                break;
        }
    }

}
