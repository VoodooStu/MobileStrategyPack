using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Core;

public class StrategyIAPManager : MonoBehaviour, IPurchaseDelegateWithInfo
{
    public static StrategyIAPManager Instance;

    public const string ExtraBuildingSlots = "com.voodoo.extrabuildingslots";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        VoodooSauce.RegisterPurchaseDelegate(this);
        Instance = this;
    }

    public void PurchaseItem(string purchase)
    {
        switch (purchase)
        {
            case ExtraBuildingSlots:
                if(StrategyDataManager.ExtraBuildingSlots< ResourceManager.Instance.Configuration.MaxAllowedExtraBuildingSlots)
                    StrategyDataManager.ExtraBuildingSlots.Value++;
                break;
        }
    }

    internal void PurchaseExtraBuilderSlot()
    {
        VoodooSauce.Purchase(ExtraBuildingSlots);
        
    }

    public void OnPurchaseComplete(ProductReceivedInfo productReceivedInfo, PurchaseValidation purchaseValidation)
    {
        if(purchaseValidation.status == ValidationStatus.Accepted)
        {
            if (purchaseValidation.isRestorationPurchase!=null && (bool)purchaseValidation.isRestorationPurchase == true)
            {
                if(StrategyDataManager.ExtraBuildingSlots<ResourceManager.Instance.Configuration.MaxAllowedExtraBuildingSlots && PlayerPrefs.GetInt("VD_AlreadyRestored"+productReceivedInfo.ProductId, 0) == 0)
                {
                    PurchaseItem(productReceivedInfo.ProductId);
                    PlayerPrefs.SetInt("VD_AlreadyRestored" + productReceivedInfo.ProductId, 1);
                }
            }
            else
            {
                PurchaseItem(productReceivedInfo.ProductId);
            }
        }
    }

    public void OnPurchaseFailure(VoodooPurchaseFailureReason reason, [CanBeNull] ProductReceivedInfo productReceivedInfo)
    {
      
    }

    public void OnInitializeSuccess()
    {
       
    }

    public void OnInitializeFailure(VoodooInitializationFailureReason reason)
    {
       
    }
}

