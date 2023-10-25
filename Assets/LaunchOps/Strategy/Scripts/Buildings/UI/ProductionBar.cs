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
    float rate = 0;
    public void Fill( BuildingDefinitionSO _data,ResourceRate _rate)
    {


        
        rate = ResourceManager.Instance.GetRate(_data, _rate);
        FillInBar();
        ResourceProductionRate.text  = rate.ToReadableString();
        ResourceIcon.sprite = ResourceManager.Instance.GetResourceIcon(_rate.Type);  
    }
    void FillInBar()
    {
        if (rate <= 0)
        {
            UpgradePercentage.text = ((int)(0)).ToString() + "%";
            UpgradeFill.fillAmount = 0f;
        }
        else
        {
            UpgradePercentage.text = ((int)(ResourceManager.Instance.CurrentProductionPercentage * 100f)).ToString() + "%";
            UpgradeFill.fillAmount = (float)ResourceManager.Instance.CurrentProductionPercentage;
        }
       
    }

    private void Update()
    {
        FillInBar();
    }
}
