using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyManager : MonoBehaviour
{
    [Header("Debug Values")]

    public bool DebugResources = false;
    public bool ResourceDebug
    {
        get
        {
#if UNITY_EDITOR
            return DebugResources;
#endif
            return false;
        }
    }

    public static StrategyManager Instance => _instance;
    private static StrategyManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        StrategyDataManager.InitialiseResources();

    }


}
