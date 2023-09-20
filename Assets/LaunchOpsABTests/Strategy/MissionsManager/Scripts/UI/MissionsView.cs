using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionsView : MonoBehaviour
{
    public MissionView missionViewPrefab;
    public Transform missionViewParent;
    public List<MissionView> missionViews = new List<MissionView>();
 
    public GameObject ClaimAllButton;
    bool hasClaimableMissions = false;
    List<MissionSO> currentMissions;
    private void Start()
    {

        MissionsManager.Instance.OnMissionsUpdated += OnMissionsUpdated;

        Hide();
    }
    private void OnDestroy()
    {
        MissionsManager.Instance.OnMissionsUpdated -= OnMissionsUpdated;
    }
    private void OnMissionsUpdated()
    {
        Refresh();
    }

    private void Refresh()
    {
        var oldClaim = hasClaimableMissions;
        Fill(currentMissions);
        hasClaimableMissions = oldClaim;
    }

    public void Fill(List<MissionSO> missions)
    {
        currentMissions= missions;
        while (missionViews.Count > 0)
        {
            Destroy(missionViews[0].gameObject);
            missionViews.RemoveAt(0);
        }
        foreach(var mission in missions)
        {
            var view = Instantiate(missionViewPrefab, missionViewParent);
            missionViews.Add(view);
            view.Fill(mission);
        }
        hasClaimableMissions = MissionsManager.Instance.MissionToClaim();
        ClaimAllButton.SetActive(hasClaimableMissions);

    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
        
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
        Fill(MissionsManager.Instance.CurrentSeasonMissions);
    }
    public void ClaimAllClick()
    {
        MissionsManager.Instance.ClaimAll();
    }
   
}
