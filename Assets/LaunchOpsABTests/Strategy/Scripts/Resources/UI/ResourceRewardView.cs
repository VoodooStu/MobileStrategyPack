using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceRewardView : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Amount;

    internal void Fill(ResourceAmount reward)
    {
        Icon.sprite = ResourceManager.Instance.GetResourceIcon(reward.type);
        Amount.text ="X"+ ((int)reward.amount).ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
