using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceRequiredView : MonoBehaviour
{
    public TextMeshProUGUI _typeTitle;
    public TextMeshProUGUI _amount;
    public Image _fillAmount;
    public GameObject _amountNeeded;
    public Image _Icon;
    public Image _background;
    public void Fill(ResourceType resourceType, float amount, Color background)
    {
        _background.color = background;
        _amount.text = amount.ToReadableString() + "/" + StrategyDataManager.GetResource(resourceType).ToReadableString();
        _typeTitle.text = resourceType.ToString();
        _fillAmount.fillAmount = StrategyDataManager.GetResource(resourceType) / amount;
       
        _Icon.sprite = ResourceManager.Instance.GetResourceIcon(resourceType);
    }


}
