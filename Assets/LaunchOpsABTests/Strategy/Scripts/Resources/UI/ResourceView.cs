using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceView : MonoBehaviour
{
    public TextMeshProUGUI Amount;
    public Image Icon;

    public void Fill(ResourceType type, float amount)
    {
        Icon.sprite = ResourceManager.Instance.GetResourceIcon(type);
        Amount.text = ((int)amount).ToString();
    }

    internal void Fill(ResourceAmount resource)
    {
        Fill(resource.type, resource.amount);
    }
}
