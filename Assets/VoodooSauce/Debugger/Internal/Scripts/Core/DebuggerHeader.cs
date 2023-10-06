using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    public class DebuggerHeader : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameObject _devBuild;

        private void Awake()
        {
            if (_backButton) {
                _backButton.onClick.AddListener(Debugger.Previous);
            }

            if (_devBuild) {
                _devBuild.SetActive(Debug.isDebugBuild);
            }
        }

        public void UpdateTarget(Screen screen) 
        {
            _title.text = screen.title;
            _icon.enabled = screen.image != null;
            _icon.sprite = screen.image;
            _icon.color = screen.imageColor;
        }
    }
}
