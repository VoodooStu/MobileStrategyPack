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
   
    private BuildingDefinitionSO Data;
    private BuildingDefinitionSO SubBuildingData;
   
  
    public BuildingStatusHeaderView HeaderView;
    [Header("Main Building Upgrade Area")]
    public Button MainBuildingUpgradeButton;
    public TextMeshProUGUI MainBuildingUpgradeButtonText;

    public UpgradeRequirementArea UpgradeRequirementArea;
    [Header("Upgrade Sub Building Button")]
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
    public TextMeshProUGUI SkipUpgradeCurrencyText;


    List<BuildingBaseStatusGroup> BuildingStatusViews = new List<BuildingBaseStatusGroup>();

    private bool IsShowing = false;
    int CurrentLevel;
    private void Awake()
    {   
        // Adding all the status views to a list to be able to switch between them
        BuildingStatusViews.AddRange(this.gameObject.GetComponents<BuildingBaseStatusGroup>());
        Hide();
        
    }
    private void Start()
    {
        // Registering to events
        StrategyEvents.OnBuildingLevelChanged += OnBuildingLevelChanged;
        StrategyEvents.OnBuildingUpgradeStart += OnBuildingBeginsUpgrade;
        StrategyEvents.OnBuildingUpgradeCancelled += OnBuildingUpgradeCancelled;

    }
    private void OnDestroy()
    {
        // Unregistering from events
        StrategyEvents.OnBuildingLevelChanged -= OnBuildingLevelChanged;
        StrategyEvents.OnBuildingUpgradeStart -= OnBuildingBeginsUpgrade;
        StrategyEvents.OnBuildingUpgradeCancelled -= OnBuildingUpgradeCancelled;

    }
    /// <summary>
    /// Called when a user cancels an upgrade mid way through
    /// </summary>
    /// <param name="building"></param>
    private void OnBuildingUpgradeCancelled(BuildingDefinitionSO building)
    {
        if (building != Data)
            return;

        if (!IsShowing)
        {
            return;
        }
        
        if (Data == building)
        {
            Fill(Data);
            StartUpgradeProgress();
        }
    }
    /// <summary>
    /// Refresh the UI if a sub building or the currently selected building has been upgraded
    /// </summary>
    /// <param name="building"></param>
    private void OnBuildingLevelChanged(BuildingDefinitionSO building)
    {

        if(!(building == Data || Data.HasSubBuilding(building)))
        {
            return;
        }

        if (!IsShowing)
        {
            return;
        }
        var previousSub = SubBuildingData;
        if (Data.Level != CurrentLevel)
            Show(Data);
        else
        {
            Fill(Data);
            if (previousSub != null)
            {
                SelectSubBuilding(previousSub);
            }
        }
           
    }
    /// <summary>
    /// Called when the main building begins an upgrade that takes time
    /// </summary>
    /// <param name="building"></param>
    private void OnBuildingBeginsUpgrade(BuildingDefinitionSO building)
    {
        if (building != Data)
            return;

        if (!IsShowing)
        {
            return;
        }
        Fill(building);
    }



    /// <summary>
    /// Current View Type of the screen. Extend this and BuildingBaseStatusGroup to add custom views.
    /// </summary>
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


        if (isMaxedOut)
        {
           // UpgradeButton.gameObject.SetActive(false);
            HeaderView.SetState(BuildingHeaderState.Maxed);
            //MaxedImage.SetActive(true);
           
          
        }
        else if (isUpgrading)
        {
            SetUpUpgrading();
          
            viewType = BuildingViewType.UpgradingBuilding;
        }
        else
        {
            UpgradeRequirementArea.Fill(Data);

            if (!Data.IsUnlocked)
            {
                HeaderView.SetState(BuildingHeaderState.Build);
                viewType = BuildingViewType.BuildBuilding;
                SelectMainBuilding();
                UpgradeRequirementArea.Fill(Data);
            }
            else if (canUpgradeMain && isRestricted)
            {
             
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
               
            }
            else
            {
                HeaderView.SetState(BuildingHeaderState.UpgradeUnAvailable);
            }
            MainBuildingUpgradeButton.interactable = StrategyDataManager.CanAfford(BuildingManager.Instance.GetUpgradePrice(Data));
            ResourceAmount amount = BuildingManager.Instance.GetUpgradePrice(Data)[0];
            TimeSpan span = new TimeSpan(0, 0, BuildingManager.Instance.GetUpgradeTime(Data));
            MainBuildingUpgradeButtonText.text = span.ToString(@"hh\:mm\:ss");
          
            SkipUpgradeCurrencyText.spriteAsset = ResourceManager.Instance.GetResourceSpriteAsset(ResourceType.Special);
            SkipUpgradeCurrencyText.text = "<sprite=0> " + BuildingManager.Instance.GetGemsRequired(Data);

        }
        UpgradeBonusText.text = BuildingManager.Instance.BuildingGlobalConfig.GetCurrentPowerLevel() + "<color=green>+" + BuildingManager.Instance.BuildingGlobalConfig.GetPowerLevel(Data.BuildingType);

        SwitchView(viewType);

        MainArea.gameObject.SetActive(false);

        MainArea.gameObject.SetActive(true);

    }

    private void SwitchView(BuildingViewType view)
    {
       
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
        HeaderView.SetState(BuildingHeaderState.BeginUpgrade);
        SwitchView(viewType);
        SelectMainBuilding();
    }

    public void OnClickBack()
    {
        Fill(Data);
    }

    public void EndUpgradeProgress()
    {
        Fill(Data);
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
        
        this.gameObject.SetActive(true);
        IsShowing = true;
        Fill(building);
        var rect = MainArea.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);
        LayoutRebuilder.MarkLayoutForRebuild(MainArea.GetComponent<RectTransform>());
        await Task.Yield();
       
       
    }
    internal void Hide()
    {
        IsShowing = false;
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
