using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;                             
using UnityEngine;
using UnityEngine.UI;

public class BuildingStatusView : MonoBehaviour
{
    [Header("Building Info Area")]
    public GameObject MainBuildingArea;
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
    [Header ("Building Status Indicators")]
   

    public TextMeshProUGUI MainBuildingUpgradeProgressText;
    public Image MainBuildingUpgradeProgressFill;

    public GameObject UpgradeAvailableImage;
    public GameObject MaxedImage;
    public GameObject UpgradeMasterIcon;
    public GameObject UpgradingImage;
    [Header("Main Building Upgrade Area")]
    public Button MainBuildingUpgradeButton;
    public TextMeshProUGUI MainBuildingUpgradeButtonText;

    public UpgradeRequirementArea UpgradeRequirementArea;
    [Header("Upgrade Button")]
    public Button UpgradeButton;
    public TextMeshProUGUI UpgradeButtonText;
    public Image UpgradeResorceIcon;
    [Header("Building Timer Area")]
    public Transform BuildingTimerParent;
    public BuildingTimer BuildingTimerPrefab;
    [Header("Upgrade Bonus Area")]
    public GameObject UpgradeBonusArea;
    public TextMeshProUGUI UpgradeBonusText;
    [Header("Skip Upgrade Button")]

    public GameObject SkipUpgardeButton;
    public Image SkipUpgradeCurrencyIcon;
    public TextMeshProUGUI SkipUpgradeCurrencyText;


    private void Awake()
    {
        Hide();
    }
    private void Start()
    {
        StrategyDataManager.OnBuildingLevelChanged += OnBuildingLevelChanged;
        BuildingManager.Instance.OnBuildingBeginsUpgrade += OnBuildingBeginsUpgrade;
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingLevelChanged;

    }

    private void OnBuildingLevelChanged(BuildingDefinitionSO sO)
    {

        var previousSub = SubBuildingData;
        Fill(Data);
        SelectSubBuilding(previousSub);
    }

    private void OnBuildingBeginsUpgrade(BuildingDefinitionSO building)
    {
        Fill(Data);
    }

    private void OnDestroy()
    {
        StrategyDataManager.OnBuildingLevelChanged -= OnBuildingLevelChanged;
        BuildingManager.Instance.OnBuildingBeginsUpgrade -= OnBuildingBeginsUpgrade;
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
        MainBuildingTitle.text = _data.DisplayName + " Lv." + _data.Level;

        bool isMaxedOut = Data.IsMaxLevel;
        bool isRestricted = BuildingManager.Instance.IsRestricted(Data);
        bool isUpgrading = BuildingManager.Instance.IsUpgrading(Data);
        bool canUpgradeMain = BuildingManager.Instance.CanUpgradeMainBuilding(Data);
        int UpgradeProgress = BuildingManager.Instance.GetMainBuildingUpgradePercentage(Data);

        // Remove old data
        while (SubBuildingViews.Count > 0)
        {
            Destroy(SubBuildingViews[0].gameObject);
            SubBuildingViews.RemoveAt(0);
        }
        while (ProductionBarViews.Count > 0)
        {
            Destroy(ProductionBarViews[0].gameObject);
            ProductionBarViews.RemoveAt(0);
        }

        // Fill in upgrade constraints
        if (Data.UpgradeConstraints.Count > 0)
        {
            SubBuildingContainer.gameObject.SetActive(true);
            UpgradeButton.gameObject.SetActive(true);
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
        }
        else
        {
            SubBuildingData = null;
            SubBuildingContainer.gameObject.SetActive(false);
           SelectMainBuilding();
            UpgradeButton.gameObject.SetActive(false);
        }

        // Setting default values
        UpgradeBonusArea.SetActive(false);
        SkipUpgardeButton.SetActive(false);
        UpgradeRequirementArea.gameObject.SetActive(true);
        MainBuildingArea.SetActive(true);
        MainBuildingUpgradeButton.gameObject.SetActive(false);
        UpgradeAvailableImage.SetActive(false);
        UpgradeMasterIcon.SetActive(false);
        MaxedImage.SetActive(false);
        BuildingTimerParent.gameObject.SetActive(false);

        if (isMaxedOut)
        {
            UpgradeButton.gameObject.SetActive(false);

            MaxedImage.SetActive(true);
            UpgradeRequirementArea.gameObject.SetActive(false);
          
        }
        else if (BuildingManager.Instance.IsUpgrading(Data))
        {
            SetUpUpgrading();
            UpgradeRequirementArea.gameObject.SetActive(false);
            MainBuildingArea.SetActive(false);
            BuildingTimerParent.gameObject.SetActive(true);
        }
        else
        {
            UpgradeRequirementArea.Fill(Data);
         
            UpgradingImage.gameObject.SetActive(false);
            MainBuildingUpgradeProgressText.gameObject.SetActive(true);

          
            if (canUpgradeMain && isRestricted)
            {
                UpgradeMasterIcon.SetActive(true);
            }
            else
            {
                MainBuildingUpgradeProgressText.text = UpgradeProgress + "%";
                MainBuildingUpgradeProgressFill.fillAmount = (float)UpgradeProgress / 100f;
                UpgradeAvailableImage.SetActive(canUpgradeMain);
                MainBuildingUpgradeButton.gameObject.SetActive(canUpgradeMain);
                MainBuildingUpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(Data));
                ResourceAmount amount = BuildingManager.Instance.GetUpgradePrice(Data)[0];
                TimeSpan span = new TimeSpan(0, 0, BuildingManager.Instance.GetUpgradeTime(Data));
                MainBuildingUpgradeButtonText.text = span.ToString(@"hh\:mm\:ss");

                //MainBuildingUpgradeButtonText.text = string.Format("<sprite={0}>", (int)amount.type) + amount.amount.ToReadableString() + "/" + StrategyDataManager.GetResource(amount.type).ToReadableString();
            }
        }

       
       
      

    }
    IEnumerator ICheckGems;
    IEnumerator iCheckGems()
    {
        while (Data != null)
        {
           
            DateTime upgradeStarted = Data.LastUpgradeTime;
            DateTime upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(Data));
            TimeSpan timeSpan = upgradeFinishes - DateTime.UtcNow;
            SkipUpgradeCurrencyText.text = "<sprite=2> " + ResourceManager.Instance.GetGemsRequired(timeSpan).ToString();
            yield return null;

        }

    }


    void SetUpUpgrading()
    {
        if(ICheckGems!=null)
        {
            StopCoroutine(ICheckGems);
            ICheckGems = null;
        }
        ICheckGems = iCheckGems();
        StartCoroutine(ICheckGems);
        UpgradeBonusArea.SetActive(true);
        SkipUpgardeButton.SetActive(true);
      
        int powerAmount = BuildingManager.Instance.BuildingGlobalConfig.GetPowerLevel(Data.BuildingType);
        UpgradeBonusText.text = BuildingManager.Instance.BuildingGlobalConfig.GetCurrentPowerLevel() + "<color=green>+" + BuildingManager.Instance.BuildingGlobalConfig.GetPowerLevel(Data.BuildingType);
        SelectMainBuilding();
        SubBuildingContainer.gameObject.SetActive(false);
        BuildingTimerParent.gameObject.SetActive(true);
        BuildingTimerPrefab.Fill(Data);
        UpgradingImage.gameObject.SetActive(true);
        UpgradeButton.gameObject.SetActive(false);
        MainBuildingUpgradeProgressText.gameObject.SetActive(false);
    }



    public void SelectMainBuilding()
    {

        SubBuildingTitle.text = Data.DisplayName;
        BuildingDescription.text = Data.BuildingDescription;
        SubBuildingIcon.sprite = Data.Icon;
        while (ProductionBarViews.Count > 0)
        {
            Destroy(ProductionBarViews[0].gameObject);
            ProductionBarViews.RemoveAt(0);
        }

    }
    /// <summary>
    /// Sub building select logic to allow for sub building upgrade
    /// </summary>
    /// <param name="building"></param>
    public void SelectSubBuilding(BuildingDefinitionSO building)
    {
        if (building == null)
            return;
        SubBuildingData = building;
        SubBuildingTitle.text = building.DisplayName;
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
        UpgradeButtonText.text = (upgradeAmount).ToReadableString() + "/" + resourceAmount.ToReadableString();
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

        if (ICheckGems != null)
        {
            StopCoroutine(ICheckGems);
            ICheckGems = null;
        }
        this.gameObject.SetActive(false);
    }

    public void Close()
    {
        StrategyMapCameraControls.Instance.SetDefaultCamera();
        BuildingManager.Instance.SelectBuilding(null);
        SubBuildingData = null;
        Hide();
    }

   

}
