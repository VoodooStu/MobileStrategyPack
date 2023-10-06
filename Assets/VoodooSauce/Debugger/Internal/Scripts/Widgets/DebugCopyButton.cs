using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Extension;

#pragma warning disable 0649

public class DebugCopyButton : MonoBehaviour
{
    [SerializeField] List<Text> _values;
    [SerializeField] Button _copyButton;

    public event System.Action buttonClicked;

    void Start() => _copyButton?.onClick.AddListener(OnButtonClick);

    void OnButtonClick()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (Text value in _values) {
            stringBuilder.Append(value.text);
            stringBuilder.Append(Environment.NewLine); 
        }
        stringBuilder.ToString().CopyToClipboard(); 
        buttonClicked?.Invoke();
    }
}