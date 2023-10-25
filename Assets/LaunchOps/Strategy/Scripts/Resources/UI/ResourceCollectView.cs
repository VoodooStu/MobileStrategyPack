using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCollectView : MonoBehaviour
{
    public Transform resourceParent;

    public ResourceView resourceView;
    private List<ResourceView> resourceViews = new List<ResourceView>();
    Dictionary<ResourceType, float> resourcesToCollect = new Dictionary<ResourceType, float>();
    [Header("Time Offline Area")]
    public TextMeshProUGUI _timer;
    public TextMeshProUGUI _maxTimeAllowed;
    public Image _timerFill;
    DateTime CalculatedTime;

    public void Fill(Dictionary<ResourceType,float> resources,DateTime calculatedTime)
    {
        CalculatedTime = calculatedTime;
        foreach(var resource in resources)
        {
            if(resource.Value == 0)
            {
                continue;
            }
            var view = Instantiate(resourceView, resourceParent);
            view.Fill(resource.Key, resource.Value);
            resourceViews.Add(view);
            view.gameObject.SetActive(true);
        }
        resourcesToCollect = resources;


        _maxTimeAllowed.text = "Max offline income: " + (new TimeSpan(0, 0, ResourceManager.Instance.Configuration.MaxGenerationTime)).TotalHours + "hrs";


    }

    public void UpdateTimer()
    {
        DateTime maxTime = CalculatedTime.AddSeconds(ResourceManager.Instance.Configuration.MaxGenerationTime);
        TimeSpan timeSpan =DateTime.UtcNow - CalculatedTime;
        if (timeSpan.TotalSeconds > ResourceManager.Instance.Configuration.MaxGenerationTime)
        {
            timeSpan = new TimeSpan(0, 0, ResourceManager.Instance.Configuration.MaxGenerationTime);
        }
        _timer.text = timeSpan.ToString(@"hh\:mm\:ss");
        _timerFill.fillAmount =((float)timeSpan.TotalSeconds / (float)ResourceManager.Instance.Configuration.MaxGenerationTime);
    }

    private void Update()
    {
        UpdateTimer();

       
    }

    public void CollectResources()
    {
        foreach(var resource in resourcesToCollect)
        {
            StrategyDataManager.AddResource(resource.Key, resource.Value,"offline_earnings");
        }
        Hide();
    }
    private void Awake()
    {
        Hide();
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
