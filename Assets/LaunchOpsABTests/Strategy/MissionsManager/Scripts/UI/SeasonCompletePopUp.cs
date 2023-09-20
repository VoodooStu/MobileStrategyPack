using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonCompletePopUp : MonoBehaviour
{
    public Image SeasonImage;
    public TextMeshProUGUI SeasonText;
    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
    private void Start()
    {
        Hide();
    }
    public void Fill(MissionSeason season)
    {
        SeasonImage.sprite = season.seasonImage;
        SeasonText.text = season.seasonName;
    }
}
