using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BuildingTimerPanel : MonoBehaviour
{
    public BuildingTimer BuildingTimer;
    public Transform TimerContent;
    private List<BuildingTimer> Timers = new List<BuildingTimer>();

    public GameObject PurchaseBuildSlotButton;

    Action OnClick;

    private void Awake()
    {
        Hide();
    }

    public void Show(Action onClick)
    {
        OnClick = onClick;
        this.gameObject.SetActive(true);
        
        Fill();
        PurchaseBuildSlotButton.SetActive(StrategyDataManager.ExtraBuildingSlots < 1);
        PurchaseBuildSlotButton.transform.SetAsLastSibling();
    }

    private void Fill()
    {
        while (Timers.Count > 0)
        {
            Destroy(Timers[0].gameObject);
            Timers.RemoveAt(0);
        }
        foreach (var building in BuildingManager.Instance.BuildingsBeingUpgraded)
        {
            var timer = Instantiate(BuildingTimer, TimerContent);
            Timers.Add(timer);
            timer.Fill(building);
        }
        
    }

    public void PurchaseSecondBuilder()
    {
        StrategyIAPManager.Instance.PurchaseExtraBuilderSlot();
    }

    public void Close()
    {
        OnClick?.Invoke();

        Hide();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        OnClick = null;
    }   

}
