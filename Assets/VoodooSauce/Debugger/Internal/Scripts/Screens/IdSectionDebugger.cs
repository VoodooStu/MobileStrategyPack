using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    public class IdSectionDebugger : MonoBehaviour
    {
        [SerializeField] private Text text;
        
        private string _currentId;

        public void UpdateDisplay(bool show, string prefixLabel, string id)
        {
            if (show)
            {
                Show(prefixLabel, id);
            }
            else
            {
                Hide();
            }
        }

        private void Show(string prefixLabel, string id)
        {
            gameObject.SetActive(true);
            _currentId = id;
            
            text.text = prefixLabel + id;
        }
        
        private void Hide()
        {
            gameObject.SetActive(false);
        }

        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = _currentId;
        }
    }
}