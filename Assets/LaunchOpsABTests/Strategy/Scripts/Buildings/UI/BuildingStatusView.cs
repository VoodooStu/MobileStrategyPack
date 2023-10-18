using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;                             
using UnityEngine;
using UnityEngine.UI;

public class BuildingStatusView : MonoBehaviour
{
    [Header("Building Info Area")]
    public BuildingInfoView BuildingInfo;
    public SubBuildingIconViews SubBuildingIcons;
    public GameObject MainArea;
   // public TextMeshProUGUI MainBuildingTitle;


    //public TextMeshProUGUI SubBuildingTitle;
    //public TextMeshProUGUI BuildingDescription;
    //public Image UpgradeProgressFill;

    //public Image SubBuildingIcon;
 
   // public SubBuildingView SubBuildingPrefab;
   // public Transform SubBuildingContainer;
    private BuildingDefinitionSO Data;
    private BuildingDefinitionSO SubBuildingData;
   
  
    public BuildingStatusHeaderView HeaderView;
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


    List<BuildingBaseStatusGroup> BuildingStatusViews = new List<BuildingBaseStatusGroup>();
    int CurrentLevel;
    private void Awake()
    {
        BuildingStatusViews.AddRange(this.gameObject.GetComponents<BuildingBaseStatusGroup>());
        Hide();
        
    }
    private void Start()
    {
        StrategyDataManager.OnBuildingLevelChanged += OnBuildingLevelChanged;
        BuildingManager.Instance.OnBuildingBeginsUpgrade += OnBuildingBeginsUpgrade;
        BuildingManager.Instance.OnBuildingUpgraded += OnBuildingLevelChanged;

    }

    private void OnBuildingLevelChanged(BuildingDefinitionSO building)
    {
        var previousSub = SubBuildingData;
        if (Data.Level != CurrentLevel)
            Show(Data);
        else
        {
            Fill(Data);
            if (SubBuildingData != null)
            {
                SelectSubBuilding(previousSub);
            }
        }
           
    }

    private void OnBuildingBeginsUpgrade(BuildingDefinitionSO building)
    {
        Show(building);
    }

    private void OnDestroy()
    {
        StrategyDataManager.OnBuildingLevelChanged -= OnBuildingLevelChanged;
        BuildingManager.Instance.OnBuildingBeginsUpgrade -= OnBuildingBeginsUpgrade;
        BuildingManager.Instance.OnBuildingUpgraded -= OnBuildingLevelChanged;
    }

    private void OnBuildingLevelChanged()
    {

        var previousSub = SubBuildingData;
        if (Data.Level != CurrentLevel)
            Show(Data);
        else
        {
            Fill(Data);
            if (SubBuildingData != null)
            {
                SelectSubBuilding(previousSub);
            }
        }
    }
    public BuildingViewType viewType;
    public void Fill(BuildingDefinitionSO _data)
    {
        viewType = BuildingViewType.BuildingInfo;
        Data = _data;
        HeaderView.Fill(Data);
        BuildingInfo.Fill(Data);
        CurrentLevel = Data.Level;
        bool isMaxedOut = Data.IsMaxLevel;
        bool isRestricted = BuildingManager.Instance.IsRestricted(Data);
        bool isUpgrading = BuildingManager.Instance.IsCurrentlyUpgrading(Data);
        bool canUpgradeMain = BuildingManager.Instance.CanUpgradeMainBuilding(Data);
        int UpgradeProgress = BuildingManager.Instance.GetMainBuildingUpgradePercentage(Data);
        
        // Remove old data



        // Fill in upgrade constraints
        if (Data.UpgradeConstraints.Count > 0)
        {
            SubBuildingIcons.Fill(Data, SelectSubBuilding);
            SubBuildingIcons.gameObject.SetActive(true);


            SelectSubBuilding(Data.UpgradeConstraints[0].BuildingDefinition);
           
        }
        else
        {
            SubBuildingIcons.gameObject.SetActive(false);
           

            SubBuildingData = null;
           
           SelectMainBuilding();
          
        }

        // Setting default values
        //UpgradeBonusArea.SetActive(false);
        //SkipUpgardeButton.SetActive(false);
        //UpgradeRequirementArea.gameObject.SetActive(true);
        //MainBuildingArea.SetActive(true);
        //MainBuildingUpgradeButton.gameObject.SetActive(false);
        //UpgradeAvailableImage.SetActive(false);
        //UpgradeMasterIcon.SetActive(false);
        //MaxedImage.SetActive(false);
        //BuildingTimerParent.gameObject.SetActive(false);

        if (isMaxedOut)
        {
           // UpgradeButton.gameObject.SetActive(false);
            HeaderView.SetState(BuildingHeaderState.Maxed);
            //MaxedImage.SetActive(true);
           
          
        }
        else if (BuildingManager.Instance.IsCurrentlyUpgrading(Data))
        {
            SetUpUpgrading();
            //UpgradeRequirementArea.gameObject.SetActive(false);
            //MainBuildingArea.SetActive(false);
            //BuildingTimerParent.gameObject.SetActive(true);
            viewType = BuildingViewType.UpgradingBuilding;
        }
        else
        {
            UpgradeRequirementArea.Fill(Data);
          
            //UpgradingImage.gameObject.SetActive(false);
            //MainBuildingUpgradeProgressText.gameObject.SetActive(true);

          
            if (canUpgradeMain && isRestricted)
            {
                //UpgradeMasterIcon.SetActive(true);
                HeaderView.SetState(BuildingHeaderState.UpgradeMaster);
            }
            else if (canUpgradeMain)
            {
                viewType = BuildingViewType.CanUpgradeBuilding;
                if(Data.UpgradeConstraints.Count == 0)
                {
                    StartUpgradeProgress();
                }
                HeaderView.SetState(BuildingHeaderState.UpgradeAvailable);
               // MainBuildingUpgradeButton.gameObject.SetActive(true);
            }
            else
            {
              
                HeaderView.SetState(BuildingHeaderState.UpgradeUnAvailable);
                
               
               // MainBuildingUpgradeButton.gameObject.SetActive(false);
                
              

                //MainBuildingUpgradeButtonText.text = string.Format("<sprite={0}>", (int)amount.type) + amount.amount.ToReadableString() + "/" + StrategyDataManager.GetResource(amount.type).ToReadableString();
            }
            MainBuildingUpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(Data));
            ResourceAmount amount = BuildingManager.Instance.GetUpgradePrice(Data)[0];
            TimeSpan span = new TimeSpan(0, 0, BuildingManager.Instance.GetUpgradeTime(Data));
            MainBuildingUpgradeButtonText.text = span.ToString(@"hh\:mm\:ss");
          
            SkipUpgradeCurrencyText.spriteAsset = ResourceManager.Instance.GetResourceSpriteAsset(ResourceType.Special);
            SkipUpgradeCurrencyText.text = "<sprite=0> " + BuildingManager.Instance.GetGemsRequired(Data);

        }
        SwitchView(viewType);

        MainArea.gameObject.SetActive(false);

        MainArea.gameObject.SetActive(true);

    }

    private void SwitchView(BuildingViewType view)
    {
        Debug.LogError("Switching to " + view.ToString());
        foreach (var stat in BuildingStatusViews)
        {
            if (stat.BuildingViewType == view)
            {
                stat.ActivateGroup();
            }
        }
    }

    public void StartUpgradeProgress()
    {
        viewType = BuildingViewType.UpgradeBuilding;
        SwitchView(viewType);
        SelectMainBuilding();
    }


    IEnumerator ICheckGems;
    IEnumerator iCheckGems()
    {
        while (Data != null)
        {
            
            DateTime upgradeStarted = Data.LastUpgradeTime;
            DateTime upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(Data));
            TimeSpan timeSpan = upgradeFinishes - DateTime.UtcNow;

            SkipUpgradeCurrencyText.text = "<sprite=0> " + ResourceManager.Instance.GetGemsRequired(timeSpan).ToString();
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
      
      
        int powerAmount = BuildingManager.Instance.BuildingGlobalConfig.GetPowerLevel(Data.BuildingType);
        UpgradeBonusText.text = BuildingManager.Instance.BuildingGlobalConfig.GetCurrentPowerLevel() + "<color=green>+" + BuildingManager.Instance.BuildingGlobalConfig.GetPowerLevel(Data.BuildingType);
        SelectMainBuilding();
       
        BuildingTimerPrefab.Fill(Data);
        HeaderView.SetState(BuildingHeaderState.Upgrading);
        //UpgradingImage.gameObject.SetActive(true);
        //UpgradeButton.gameObject.SetActive(false);
        //MainBuildingUpgradeProgressText.gameObject.SetActive(false);
    }



    public void SelectMainBuilding()
    {

        //SubBuildingTitle.text = Data.DisplayName;
        //BuildingDescription.text = Data.BuildingDescription;
        //SubBuildingIcon.sprite = Data.Icon;
        BuildingInfo.Fill(Data);
        

    }

    public void SelectNextUpgradeableSubBuilding()
    {
        foreach(var sub in Data.UpgradeConstraints)
        {
            if (BuildingManager.Instance.CanUpgradeSubBuilding(Data,sub.BuildingDefinition))
            {
                SelectSubBuilding(sub.BuildingDefinition);
                return;
            }
        }
    }

    public void OnClickGems()
    {
        if (!BuildingManager.Instance.IsCurrentlyUpgrading(Data))
        {
            BuildingManager.Instance.InstantUpgrade(Data);
        }
        else
        {
            DateTime upgradeStarted = Data.LastUpgradeTime;
            DateTime upgradeFinishes = upgradeStarted.AddSeconds(BuildingManager.Instance.GetUpgradeTime(Data));
            TimeSpan timeSpan = upgradeFinishes - DateTime.UtcNow;
            int gemsRequired = ResourceManager.Instance.GetGemsRequired(timeSpan);
            if (StrategyDataManager.GetResource(ResourceType.Special) >= gemsRequired)
            {
                StrategyDataManager.RemoveResource(ResourceType.Special, gemsRequired, "Skipping_Build");
                BuildingManager.Instance.FinishUpgrade(Data);
            }
            else
            {
                StrategyUIManager.Instance.InsufficentResourcesView.Show();
                StrategyUIManager.Instance.InsufficentResourcesView.Fill(new List<ResourceAmount>() { new ResourceAmount() { type = ResourceType.Special, amount = gemsRequired } });
            }
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
        //BuildingInfo.Fill(building);
        SubBuildingData = building;

        bool otherUpgradeAvailable = false;
        foreach (var b in Data.UpgradeConstraints)
        {
            if (b.BuildingDefinition != SubBuildingData)
            {
                if (BuildingManager.Instance.CanUpgradeSubBuilding(Data, b.BuildingDefinition) && b.StartLevel <= Data.Level)
                {
                    otherUpgradeAvailable = true;
                    break;
                }
            }
        }

        SubBuildingIcons.SubBuildingChanged(building);
        BuildingInfo.Fill(building, BuildingManager.Instance.CanUpgradeSubBuilding(Data, SubBuildingData), StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(building)), otherUpgradeAvailable);
        return;





        var UpgradePrice = BuildingManager.Instance.GetUpgradePrice(building)[0];
        float upgradeAmount = UpgradePrice.amount;
        float resourceAmount = StrategyDataManager.GetResource(UpgradePrice.type);
        UpgradeResorceIcon.sprite = ResourceManager.Instance.GetResourceIcon(UpgradePrice.type);
        UpgradeButtonText.text = (upgradeAmount).ToReadableString() + "/" + resourceAmount.ToReadableString();
        UpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(building)) && BuildingManager.Instance.CanUpgradeSubBuilding(Data, SubBuildingData);



    }

    public void TryUpgradeMainBuilding()
    {
        BuildingManager.Instance.TryUpgradeBuilding(Data);
    }

    public void TryUpgradeSubBuilding()
    {
        BuildingManager.Instance.TryUpgradeBuilding(SubBuildingData);
    }

    internal async void Show(BuildingDefinitionSO building)
    {
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        Fill(building);
        var rect = MainArea.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
        LayoutRebuilder.MarkLayoutForRebuild(MainArea.GetComponent<RectTransform>());
        await Task.Yield();
       
       
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
