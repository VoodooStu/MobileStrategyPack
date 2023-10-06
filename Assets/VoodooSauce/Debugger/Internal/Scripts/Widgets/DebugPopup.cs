using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugPopup : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private Button _button;

        public void Initialize(string text, UnityAction buttonCallback = null)
        {
            _text.text = text;
            
            if (buttonCallback != null)
            {
                _button.onClick.AddListener(buttonCallback);
            }
        }
        
        public void Initialize(UnityAction buttonCallback)
        {
            if (buttonCallback != null)
            {
                _button.onClick.AddListener(buttonCallback);
            }
        }

        public void Dispose()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}