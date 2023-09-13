using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class BuildingStatusView : MonoBehaviour
{

    public TextMeshProUGUI MainBuildingTitle;

  
    public TextMeshProUGUI SubBuildingTitle;
    public TextMeshProUGUI BuildingDescription;
    public Image UpgradeProgressFill;
   
    public Image SubBuildingIcon;
    public ProductionBar ProductionBarPrefab;
    public Transform ProductionBarContainer;
    public SubBuildingView SubBuildingPrefab;
    public Transform SubBuildingContainer;
    private BuildingDefinitionSO Data;
    private BuildingDefinitionSO SubBuildingData;
    private List<SubBuildingView> SubBuildingViews = new List<SubBuildingView>();
    private List<ProductionBar> ProductionBarViews = new List<ProductionBar>();

    [Header("Main Building Upgrade Area")]
    public Button MainBuildingUpgradeButton;
    public TextMeshProUGUI MainBuildingUpgradeButtonText;
    
    public TextMeshProUGUI MainBuildingUpgradeProgressText;
    public Image MainBuildingUpgradeProgressFill;
    [Header("Upgrade Button")]
    public Button UpgradeButton;
    public TextMeshProUGUI UpgradeButtonText;
    public Image UpgradeResorceIcon;
    public GameObject MaxedImage;

    private void Awake()
    {
        Hide();
    }
    private void Start()
    {
        StrategyDataManager.OnBuildingLevelChanged += OnBuildingLevelChanged;
      
    }
    private void OnDestroy()
    {
        StrategyDataManager.OnBuildingLevelChanged -= OnBuildingLevelChanged;
    }

    private void OnBuildingLevelChanged()
    {
        var previousSub = SubBuildingData;
        Fill(Data);
        SelectSubBuilding(previousSub);
    }

    public void Fill(BuildingDefinitionSO _data)
    {

        Data = _data;
        MainBuildingTitle.text = _data.BuildingName + " Lv."+_data.Level;
        while(SubBuildingViews.Count > 0)
        {
            Destroy(SubBuildingViews[0].gameObject);
            SubBuildingViews.RemoveAt(0);
        }
       
        SelectSubBuilding(Data.UpgradeConstraints[0].BuildingDefinition);
        foreach (var building in Data.UpgradeConstraints)
        {
            if (building.StartLevel <= Data.Level)
            {
                SubBuildingView view = Instantiate(SubBuildingPrefab, SubBuildingContainer);
                SubBuildingViews.Add(view);
                view.Fill(building, SelectSubBuilding, building.BuildingDefinition == SubBuildingData);
              
            }
          
        }
        bool isMaxedOut = BuildingManager.Instance.IsMaxedOut(Data);

        if (isMaxedOut)
        {
            MainBuildingUpgradeButton.gameObject.SetActive(false);
            MaxedImage.SetActive(true);
        }
        else
        {
            MaxedImage.SetActive(false);
            bool canUpgradeMain = BuildingManager.Instance.CanUpgradeMainBuilding(Data);
            int UpgradeProgress = BuildingManager.Instance.GetMainBuildingUpgradePercentage(Data);
            MainBuildingUpgradeProgressText.text = UpgradeProgress + "%";
            MainBuildingUpgradeProgressFill.fillAmount = (float)UpgradeProgress / 100f;
            MainBuildingUpgradeButton.gameObject.SetActive(canUpgradeMain);
            MainBuildingUpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(Data));
            ResourceAmount amount = BuildingManager.Instance.GetUpgradePrice(Data)[0];

            MainBuildingUpgradeButtonText.text = string.Format("<sprite={0}>", (int)amount.type) + amount.amount.ToReadableString() + "/" + StrategyDataManager.GetResource(amount.type);
        }

    }

    public void SelectSubBuilding(BuildingDefinitionSO building)
    {
        SubBuildingData = building;
        SubBuildingTitle.text = building.BuildingName;
        BuildingDescription.text = building.BuildingDescription;
        SubBuildingIcon.sprite = building.Icon;
        while (ProductionBarViews.Count > 0)
        {
            Destroy(ProductionBarViews[0].gameObject);
            ProductionBarViews.RemoveAt(0);
        }
        foreach(var resource in building.ResourceRates)
        {
            ProductionBar view = Instantiate(ProductionBarPrefab, ProductionBarContainer);
            view.Fill(Data,building,resource, building.Level);
            ProductionBarViews.Add(view);
        }

        var UpgradePrice = BuildingManager.Instance.GetUpgradePrice(building)[0];
        float upgradeAmount = UpgradePrice.amount;
        float resourceAmount = StrategyDataManager.GetResource(UpgradePrice.type);
        UpgradeResorceIcon.sprite = ResourceManager.Instance.GetResourceIcon(UpgradePrice.type);
        UpgradeButtonText.text = upgradeAmount.ToReadableString() + "/" + resourceAmount.ToReadableString();
        UpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(building)) && BuildingManager.Instance.CanUpgradSubBuildinge(Data, SubBuildingData);

        foreach(var sub in SubBuildingViews)
        {
            sub.SetSelected(SubBuildingData);
        }

    }

    public void TryUpgradeMainBuilding()
    {
        BuildingManager.Instance.TryUpgradeBuilding(Data);
    }

    public void TryUpgradeSubBuilding()
    {
        BuildingManager.Instance.TryUpgradeBuilding(SubBuildingData);
    }

    internal void Show()
    {
        this.gameObject.SetActive(true);
    }
    internal void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Close()
    {
        StrategyMapCameraControls.Instance.SetDefaultCamera();
        BuildingManager.Instance.SelectBuilding(null);
        
        Hide();
    }

   

}
