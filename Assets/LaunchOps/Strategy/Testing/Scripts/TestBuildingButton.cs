using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestBuildingButton : MonoBehaviour
{
    private BuildingDefinitionSO Data;
    private Action<BuildingDefinitionSO> OnClick;
    public TextMeshProUGUI BuildingName;
    public void Fill(BuildingDefinitionSO _data,Action<BuildingDefinitionSO> onClick)
    {
        BuildingName.text = _data.DisplayName;
        OnClick = onClick;
        Data = _data;
    }

    public void OnClickButton()
    {
        OnClick?.Invoke(Data);
    }
}
