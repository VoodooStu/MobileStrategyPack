using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyIAPManager : MonoBehaviour
{
    public static StrategyIAPManager Instance;

    public const string ExtraBuildingSlots = "ExtraBuildingSlots";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void PurchaseItem(string purchase)
    {
        switch (purchase)
        {
            case ExtraBuildingSlots:
                StrategyDataManager.ExtraBuildingSlots.Value++;
                break;
        }
    }

    internal void PurchaseExtraBuilderSlot()
    {
        PurchaseItem(ExtraBuildingSlots);
    }
}

