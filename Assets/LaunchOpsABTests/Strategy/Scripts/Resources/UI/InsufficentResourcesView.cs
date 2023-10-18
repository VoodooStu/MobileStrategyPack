using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsufficentResourcesView : MonoBehaviour
{
    public Transform content;
    public ResourceRequiredView _requiredViewPrefab;
    private List<ResourceRequiredView> _resourceRequiredViews = new List<ResourceRequiredView>();
    public Color firstColor;
    public Color secondColor;
    public void Show()
    {
        this.gameObject.SetActive(true);
    }


    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    internal void Fill(List<ResourceAmount> price)
    {
        while (_resourceRequiredViews.Count > 0)
        {
            Destroy(_resourceRequiredViews[0].gameObject);
            _resourceRequiredViews.RemoveAt(0);
        }
        int colorChoice = 0;
        foreach (var p in price)
        {
            var view = Instantiate(_requiredViewPrefab, content);
            view.Fill(p.type, p.amount,colorChoice%2 == 0?firstColor:secondColor);
            colorChoice +=1;
            _resourceRequiredViews.Add(view);
            view.transform.SetAsFirstSibling();

        }

    }

    public void ShowMissions()
    {
        BuildingManager.Instance.SelectBuilding(null);
        StrategyUIManager.Instance.MissionsView.Show();
    }
}
