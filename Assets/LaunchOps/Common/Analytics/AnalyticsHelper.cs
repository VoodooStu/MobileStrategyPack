using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VOODOO_SAUCE
using Voodoo.Sauce.Internal.Analytics;
#endif

public static class AnalyticsHelper
{
#if VOODOO_SAUCE
    public static List<VoodooSauce.AnalyticsProvider> Providers = new List<VoodooSauce.AnalyticsProvider>() { VoodooSauce.AnalyticsProvider.GameAnalytics, VoodooSauce.AnalyticsProvider.VoodooAnalytics };
#else
//    public static List<TinySauce.AnalyticsProvider> Providers = new List<TinySauce.AnalyticsProvider>() { TinySauce.AnalyticsProvider.GameAnalytics, TinySauce.AnalyticsProvider.VoodooAnalytics };
#endif

    // Faster function to avoid adding providers + double properties everytime you create a tracking event
    public static void TrackCustomEvent(string name, Dictionary<string,object> data = null)
    {
#if VOODOO_SAUCE
        VoodooSauce.TrackCustomEvent(name, data, "custom", Providers, data);
#else
//        TinySauce.TrackCustomEvent(name, data, "custom", Providers);
#endif
    }

#region GameTracking

    public static void OnGameFinished(bool win)
    {
        var parameters = new Dictionary<string, object>
        {
            { "level_number", DataManager.GameplayLevel.Value },
            { "player_level", DataManager.PlayerLevel.Value },
            // Add any cool analytics infos here
        };
/*
        // Selected skill & level
        List<DeckCardInstance> cards = DeckManager.Instance.GetAllSelectedInstanceOfType(typeof(DeckCardExampleConfig));
        for (int i = 0; i < cards.Count; i++)
        {
            // Analytics doesn't support integer, so let's use A/B/C...
            char letter = (char)('a' + i);

            DeckCardInstance card = cards[i];
            if (card != null)
            {
                parameters.Add("skillCard_" + letter, card.Config.ID);
                parameters.Add("skillLevel_" + letter, card.GetPowerLevel());
            }
            else
            {
                parameters.Add("skillCard_" + letter, "none");
            }
        }*/

        string gameplayLevel = DataManager.GameplayLevel.Value.ToString();
#if VOODOO_SAUCE
        VoodooSauce.OnGameFinished(win, 1, gameplayLevel, parameters, parameters);
#else
//        TinySauce.OnGameFinished(win, 1, gameplayLevel, parameters);
#endif
    }

    public static void OnGameStarted()
    {
        string level = DataManager.GameplayLevel.Value.ToString();
#if VOODOO_SAUCE
        VoodooSauce.OnGameStarted(level);
#else
//        TinySauce.OnGameStarted(level);
#endif
    }

#endregion

#region Currencytracking


/// <summary>
/// Use this everytime a transaction is done in the game
/// </summary>
/// <param name="currencyAmount">Quantity (positive or negative) of currency transfered</param>
/// <param name="placement">Placement enum to know the source of this transaction. Can be a string, that will be cast
/// into the enum if possible</param>
/// <param name="currencyTransactionType">is it from normal gameplay, from IAP, or from an RV ?</param>
/// <param name="currencyEnum">Soft/Hard/other enum</param>
public static void TrackCurrencyTransaction(int currencyAmount, string placement,
    CurrencyTransactionType currencyTransactionType = CurrencyTransactionType.Gameplay,
    CurrencyEnum currencyEnum = CurrencyEnum.Soft)
{
    // Having an enum type used everywhere is messy, sometimes we want to use a simple string between our functions.
    if (Enum.TryParse(typeof(PlacementEnum), placement, true, out object result))
    {
        TrackCurrencyTransaction(currencyAmount, (PlacementEnum)result, currencyTransactionType, currencyEnum);
    }
    else
    {
        Debug.LogError("string " + placement + " doesn't exist in Placement list");
        TrackCurrencyTransaction(currencyAmount, PlacementEnum.Default, currencyTransactionType, currencyEnum);
    }
}

public static void TrackCurrencyTransaction(int currencyAmount, PlacementEnum placement,
    CurrencyTransactionType currencyTransactionType = CurrencyTransactionType.Gameplay,
    CurrencyEnum currencyEnum = CurrencyEnum.Soft)
{
#if VOODOO_SAUCE
        ItemTransactionParameters transaction = new ItemTransactionParameters();
        transaction.balance = DataManager.Soft;
        transaction.item = new ItemTransactionInfo();
        transaction.item.itemName = currencyEnum;
        transaction.item.itemType = ItemType.soft_currency;
        if (currencyEnum == CurrencyEnum.Hard)
            transaction.item.itemType = ItemType.hard_currency;
        transaction.level = DataManager.PlayerLevel.Value.ToString();
        transaction.placement = placement;
        transaction.placementId = placement.ToString();
        transaction.transactionType = currencyAmount > 0 ? TransactionType.In : TransactionType.Out;
        transaction.nbUnits = Mathf.Abs(currencyAmount);
        transaction.currencyUsed = currencyTransactionType;
        VoodooSauce.OnItemTransaction(transaction);
#else
/*    if (currencyAmount > 0)
    {
        TinySauce.OnCurrencyGiven(currencyEnum.ToString(), currencyAmount, currencyTransactionType.ToString(),
            placement.ToString());
    }
    else
    {
        TinySauce.OnCurrencyTaken(currencyEnum.ToString(), currencyAmount, currencyTransactionType.ToString(),
            placement.ToString());
    }*/
#endif
}
    
public enum CurrencyEnum
{
    Soft,
    Hard,
}

// Add new placement to this enum so that you're able to fill ItemTransaction.
public enum PlacementEnum
{
    Default,
    Chest,
    CardUpgrade,
    WinMenu,
    Shop,
}

public enum CurrencyTransactionType
{
    Gameplay,
    RV,
    IAP
}

#endregion



#region CardsEvents
/*


    // Can be called once before collecting cards to register the placement of all following events
    private static string _collectPlacement = "";
    private static string _collectType = "";
    public static void OnCardCollectStart(string source, string purchase_type)
    {
        _collectPlacement = source;
        _collectType = purchase_type;
    }

    public static void TrackCardCollected(DeckCardGenericConfig card, int amount)
    {
        DeckCardInstance cardInstance = DeckManager.Instance.GetCardInstance(card.ID);
        if (cardInstance == null)
            return; // something went wrong

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "card_name", card.ID },
            { "card_type", card.TypeToText() },
            { "card_rarity", card.Rarity.ToString() },
            { "nb_received", amount },
            { "placement", _collectPlacement },
            { "purchase_type", _collectType },
            { "current_balance", cardInstance.Save.Amount },
            { "total_cards_cumulated", cardInstance.TotalCardSpend() + cardInstance.Save.Amount },
            { "current_card_level", cardInstance.Save.Level },
            { "can_be_upgraded", cardInstance.CanUpgrade() },
        };

        TrackCustomEvent("card_collected", data);
    }

    public static void TrackCardUpgrade(DeckCardInstance card)
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "card_name", card.Config.ID },
            { "card_type", card.Config.TypeToText() },
            { "card_rarity", card.Config.Rarity.ToString() },
            { "new_card_level", card.Save.Level },
            { "cards_left_after_update", card.Save.Amount },
        };

        TrackCustomEvent("card_upgraded", data);
    }

    public static void TrackCardCollection()
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        
        foreach (DeckCardSave save in DeckManager.Instance.Save.Cards.Values)
        {
            CardCollectionAnalyticData data = new CardCollectionAnalyticData
            {
                Quantity = save.Amount,
                Level = save.Level
            };
            dic[save.Config.ID] = JsonUtility.ToJson(data);

            if (dic.Count > 31)
                break; // Security, do not send more than 31 parameters
        }

        TrackCustomEvent("card_collection", dic);

    }    
    
    [Serializable]
    public struct CardCollectionAnalyticData
    {
        public int Quantity;
        public int Level;
    }
*/
#endregion


}
