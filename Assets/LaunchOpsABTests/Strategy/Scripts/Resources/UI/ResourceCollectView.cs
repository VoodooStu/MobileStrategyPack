using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollectView : MonoBehaviour
{
    public Transform resourceParent;

    public ResourceView resourceView;
    private List<ResourceView> resourceViews = new List<ResourceView>();
    Dictionary<ResourceType, float> resourcesToCollect = new Dictionary<ResourceType, float>();
    public void Fill(Dictionary<ResourceType,float> resources)
    {
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
