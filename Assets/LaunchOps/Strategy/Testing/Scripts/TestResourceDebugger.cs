using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

public class TestResourceDebugger : MonoBehaviour
{
    public ResourceType ResourceType;

    public TMP_InputField InputField;
    public TextMeshProUGUI ResourceTitle;
    public Image Icon;
    // Start is called before the first frame update
    void Start()
    {
        InputField.onEndEdit.AddListener(OnEndEdit);
        StrategyDataManager.OnResourceChanged += Fill;
        Icon.sprite = ResourceManager.Instance.GetResourceIcon(ResourceType);
        ResourceTitle.text = ResourceType.ToString();
        Fill();
    }

    private void Fill()
    {
        InputField.text = StrategyDataManager.GetResource(ResourceType).ToString();
    }

    private void OnEndEdit(string amount)
    {
        StrategyDataManager.SetResource(ResourceType, float.Parse(amount));
    }

   
}
