using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    public class Badge : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] private Text value;

        public void Hide() => container.SetActive(false);
        
        public void Show() => container.SetActive(true);

        public void SetValue(string v) => value.text = v;
    }

    public abstract class BadgeCounter
    {
        public Action<int> update;
        public abstract void Start();
        public abstract void Stop();
    }
}