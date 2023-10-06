using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    [RequireComponent(typeof(Button))]
    public class DebugMenuItemButton : Widget
    {
        [SerializeField] private Image menuIconImage;
        [SerializeField] private Text menuNameText;
        [SerializeField] private Badge badge;
        
        private Button _menuButton;
        private Action _callback;

        private BadgeCounter _counter;

        private void Awake()
        {
            _menuButton = GetComponent<Button>();
        }

        private void OnDestroy()
        {
            if (_counter != null) {
                _counter.update -= OnBadgeValueUpdate;
            }
        }

        private void OnEnable()
        {
            _menuButton.onClick.AddListener(OnButtonClick);
            _counter?.Start();
        }

        private void OnDisable()
        {
            _menuButton.onClick.RemoveListener(OnButtonClick);
            _counter?.Stop();
        }

        public void SetIcon(Sprite icon, Color color)
        {
            menuIconImage.sprite = icon;
            menuIconImage.color = color;
        }

        public void SetText(string text)
        {
            menuNameText.text = text;
        }

        public void SetCallback(Action callback)
        {
            _callback = callback;
        }

        public void SetBadgeCounter(BadgeCounter counter)
        {
            if (_counter != null) {
                _counter.update -= OnBadgeValueUpdate;
            }
            
            _counter = counter;
            
            if (_counter == null) {
                badge.Hide();
            } else {
                _counter.update += OnBadgeValueUpdate;
                if (enabled) {
                    _counter.Start();
                } else {
                    _counter.Stop();
                }
            }
        }

        private void OnButtonClick()
        {
            _callback?.Invoke();
        }

        private void OnBadgeValueUpdate(int count)
        {
            if (count < 0) {
                badge.SetValue("???");
            } else {
                badge.SetValue($"{count}");
                if (count > 0) {
                    badge.Show();
                } else {
                    badge.Hide();
                }
            }
        }
    }
}
