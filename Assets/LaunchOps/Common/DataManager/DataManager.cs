using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataManager
{
    /// <summary>
    /// This file gather the player save data common to most games, like currencies/levels...
    /// </summary>
    
    
    // Player Soft currency.
    public static Action OnSoftChanged;
    // Private to avoid changing it without using Spend function, which call all analytics stuff
    private static SavedInt _soft = new SavedInt("VD_Cur_Soft", 0, true, () =>
    {
        OnSoftChanged?.Invoke();
    });
    public static int Soft => _soft.Value;

    // Player Hard currency
    public static Action OnHardChanged;
    // Private to avoid changing it without using Spend function, which call all analytics stuff
    private static SavedInt _hard = new SavedInt("VD_Cur_Hard", 0, true, () =>
    {
        OnHardChanged?.Invoke();
    });
    public static int Hard => _hard.Value;
    
    // Total spent currencies
    public static SavedInt SpentSoft = new SavedInt("VD_SpentSoft", 0, true, () => { });
    public static SavedInt SpentHard = new SavedInt("VD_SpentHard", 0, true, () => { });

    public static void AddSoft(int amount, string source, AnalyticsHelper.CurrencyTransactionType type = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        amount = Mathf.Max(-_soft.Value, amount); // Avoid going under 0
        
        _soft.Value += amount;
        if (amount < 0)
            SpentSoft.Value += amount;
        AnalyticsHelper.TrackCurrencyTransaction(amount, source, type, AnalyticsHelper.CurrencyEnum.Soft);
    }

    public static void AddHard(int amount, string source, AnalyticsHelper.CurrencyTransactionType type = AnalyticsHelper.CurrencyTransactionType.Gameplay)
    {
        amount = Mathf.Max(-_hard.Value, amount); // Avoid going under 0
        
        _hard.Value += amount;
        if (amount < 0)
            SpentHard.Value += amount;
        AnalyticsHelper.TrackCurrencyTransaction(amount, source, type, AnalyticsHelper.CurrencyEnum.Hard);
    }

    
    // In a level-based game, this gets incremented after every victory
    public static Action OnGameplayLevelChanged;
    public static SavedInt GameplayLevel = new SavedInt("VD_GameplayLevel", 0, true, () =>
    {
        OnGameplayLevelChanged?.Invoke();
    });

    // This can be linked to any specific system (like leagues) or an experience system
    public static Action OnPlayerLevelChanged;
    public static SavedInt PlayerLevel = new SavedInt("VD_PlayerLevel", 0, true, () =>
    {
        OnPlayerLevelChanged?.Invoke();
    });
    
    public static SavedString Pseudo = new SavedString("VD_PlayerPseudo", "Player#" + UnityEngine.Random.Range(10, 9999).ToString(), true, null);

    public static Action OnEnableMusicChanged;
    public static SavedBool EnableMusic = new SavedBool("VD_EnableMusic", true, true, () =>
    {
        OnEnableMusicChanged?.Invoke();
    });

    public static Action OnEnableSFXChanged;
    public static SavedBool EnableSFX = new SavedBool("VD_EnableSFX", true, true, () =>
    {
        OnEnableSFXChanged?.Invoke();
    });

    public static Action OnEnableHapticChanged;
    public static SavedBool EnableHaptic = new SavedBool("VD_EnableHaptic", true, true, () =>
    {
        OnEnableHapticChanged?.Invoke();
    });
}