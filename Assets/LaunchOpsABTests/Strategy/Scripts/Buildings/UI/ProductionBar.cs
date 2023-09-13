using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionBar : MonoBehaviour
{
   
    public TextMeshProUGUI ResourceProductionRate;
    public Image ResourceIcon;
    public TextMeshProUGUI ResourceName;
    public TextMeshProUGUI UpgradePercentage;
    public Image UpgradeFill;
    private BuildingDefinitionSO Data;

    public void Fill(BuildingDefinitionSO _mainBuilding, BuildingDefinitionSO _data,ResourceRate _rate,int maxLevel)
    {
        int upgradePercentage = BuildingManager.Instance.GetSubBuildingUpgradePercentage(_mainBuilding, _data);

        UpgradePercentage.text = upgradePercentage.ToString() + "%";
        UpgradeFill.fillAmount = (float)upgradePercentage / 100f;

        ResourceProductionRate.text  = ResourceManager.Instance.GetRate(_data, _rate).ToReadableString();
        ResourceIcon.sprite = ResourceManager.Instance.GetResourceIcon(_rate.Type);  
    }
}
