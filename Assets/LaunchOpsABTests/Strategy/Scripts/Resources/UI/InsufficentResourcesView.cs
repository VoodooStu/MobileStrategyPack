using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsufficentResourcesView : MonoBehaviour
{
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
}
