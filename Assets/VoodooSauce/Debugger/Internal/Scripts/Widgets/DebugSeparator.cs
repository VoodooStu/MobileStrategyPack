using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.DebugScreen;

namespace Voodoo.Sauce.Debugger.Widgets
{
    public class DebugSeparator : Widget, IDebugRefreshable
    {
        [SerializeField] private Image separatorImage;
        
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetSize(int height)
        {
            Vector2 size = _rectTransform.sizeDelta;
            size.y = height;
            _rectTransform.sizeDelta = size;
        }

        public void DisplayLine(bool value = true)
        {
            separatorImage.enabled = value;
        }

        public void Refresh()
        {
            
        }
    }
}
