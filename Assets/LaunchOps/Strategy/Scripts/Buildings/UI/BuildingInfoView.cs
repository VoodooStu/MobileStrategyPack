using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoView : MonoBehaviour
{
    [Header("UI Elements")]

    public TextMeshProUGUI BuildingName;
    public TextMeshProUGUI BuildingDescription;
    public TextMeshProUGUI UpgradeCostText;
    public Button UpgradeButton;
    public Button NextBuildingButton;
    public Image UpgradeResorceIcon;
    private BuildingDefinitionSO BuildingDefinitionSO;
    public ProductionBar ProductionBarPrefab;
    public Transform ProductionBarContainer;
    List<ProductionBar> ProductionBars = new List<ProductionBar>();
    public void Fill(BuildingDefinitionSO building,bool canUpgrade,bool canAffordUpgrade,bool nextSubBuildingAvailable)
    {

        BuildingDefinitionSO = building;
        FillInText();
        bool showUpgradeButton = canUpgrade || !nextSubBuildingAvailable;
        UpgradeButton.gameObject.SetActive(showUpgradeButton);
        UpgradeButton.interactable = canAffordUpgrade && canUpgrade;
        NextBuildingButton.gameObject.SetActive(!showUpgradeButton);
        FillInUpgradeButton();
        FillInProductionBars();




    }

    private void FillInProductionBars()
    {
        while (ProductionBars.Count > 0)
        {
            Destroy(ProductionBars[0].gameObject);
            ProductionBars.RemoveAt(0);
        }
        foreach(var production in BuildingDefinitionSO.ResourceRates)
        {
            var productionBar = Instantiate(ProductionBarPrefab, ProductionBarContainer);   
            productionBar.Fill(BuildingDefinitionSO, production);
            ProductionBars.Add(productionBar);
        }
        ProductionBarContainer.gameObject.SetActive(ProductionBars.Count > 0);
    }

    internal void Fill(BuildingDefinitionSO building)
    {
        BuildingDefinitionSO = building;
        FillInText();
        UpgradeButton.gameObject.SetActive(false);
        NextBuildingButton.gameObject.SetActive(false);
        FillInProductionBars();
    }

    void FillInText()
    {
      
        BuildingName.text = BuildingDefinitionSO.DisplayName;
        BuildingDescription.text = BuildingDefinitionSO.BuildingDescription;
    }

    void FillInUpgradeButton()
    {
        var UpgradePrice = BuildingManager.Instance.GetUpgradePrice(BuildingDefinitionSO)[0];
        float upgradeAmount = UpgradePrice.amount;
        float resourceAmount = StrategyDataManager.GetResource(UpgradePrice.type);
        UpgradeResorceIcon.sprite = ResourceManager.Instance.GetResourceIcon(UpgradePrice.type);
        UpgradeCostText.text = (upgradeAmount).ToReadableString() + "/" + resourceAmount.ToReadableString();
    }

}
