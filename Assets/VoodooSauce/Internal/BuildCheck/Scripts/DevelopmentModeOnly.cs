using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal;

public class DevelopmentModeOnly : MonoBehaviour
{
    public Text developmentModeTextUI;
    private const string DEFAULT_DEVELOPMENT_TEXT = "DEVELOPMENT BUILD";
    private const string DEVELOPMENT_TEXT_WITH_SUPER_PREMIUM = "DEVELOPMENT BUILD \nwith Super Premium";
    private void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (VoodooSuperPremium.IsSuperPremium()) {
            developmentModeTextUI.text = DEVELOPMENT_TEXT_WITH_SUPER_PREMIUM;
        } else {
            developmentModeTextUI.text = DEFAULT_DEVELOPMENT_TEXT;
        }
    }
}
