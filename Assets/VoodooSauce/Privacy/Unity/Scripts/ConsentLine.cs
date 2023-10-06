using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    public class ConsentLine : MonoBehaviour
    {
        [SerializeField]
        private SimpleCheckbox _checkbox;
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private Button _button;

        public class Parameters
        {
            public string text;
            public UnityAction buttonAction;
            public bool isChecked;
            public bool isLocked;
            public UnityAction<bool> toggleEvent;
        }

        public void Initialize(Parameters p)
        {
            _text.text = p.text;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(p.buttonAction);
            _checkbox.Initialize(p.isChecked, p.isLocked, p.toggleEvent);
        }

        public bool IsChecked()
        {
            return _checkbox.IsChecked();
        }

        public void AddButtonListener(UnityAction action)
        {
            _button.onClick.AddListener(action);
        }
    }
}
