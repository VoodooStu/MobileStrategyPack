using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCounter : MonoBehaviour
{
    public ResourceType ResourceType;
    public TextMeshProUGUI AmountText;
    public Image Icon;

    private void Start()
    {
        StrategyDataManager.OnResourceChanged += Fill;
        Fill();
    }
    private void OnDestroy()
    {
        StrategyDataManager.OnResourceChanged -= Fill;
    }

    public void Fill() { 
    
        AmountText.text = StrategyDataManager.GetResource(ResourceType).ToReadableString();
        Icon.sprite = ResourceManager.Instance.GetResourceIcon(ResourceType);
    }

}
