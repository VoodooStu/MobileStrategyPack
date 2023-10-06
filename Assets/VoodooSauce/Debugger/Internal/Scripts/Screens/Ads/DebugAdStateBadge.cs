using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public abstract class DebugAdStateBadge : MonoBehaviour
    {
        [SerializeField]
        private GameObject badgeObject;
        
        [SerializeField]
        private Image stateObject;

        protected abstract bool IsEnabled();
        protected abstract Color StateColor();

        protected virtual void Awake()
        {
            UpdateVisibility();
        }

        protected void UpdateVisibility()
        {
            badgeObject.SetActive(IsEnabled());
        }

        private void Update()
        {
            stateObject.color = StateColor();
        }
    }
}