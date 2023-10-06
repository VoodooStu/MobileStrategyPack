using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Tools.AccessButton;

namespace Voodoo.Sauce.Debugger
{
    [RequireComponent(typeof(Canvas))]
    public class DebuggerCanvas : MonoBehaviour
    {
        [SerializeField] private DebuggerCanvasData _data;
        [SerializeField] private HomeDebugScreen _homePrefab;
        [SerializeField] private DebuggerHeader _header;
        [SerializeField] private Transform _bodyRoot;
        [SerializeField] private Transform _hiddenRoot;
        [SerializeField] private DebuggerPopupScreen _popupScreen;
        [SerializeField] private CustomDebuggerScreen _customDebuggerScreen;

        private Canvas _canvas;
        private Stack<Screen> _screenQueue = new Stack<Screen>();

        public bool IsOpened => _canvas.enabled;
        public bool IsHome => _screenQueue.Count <= 1;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _canvas = GetComponent<Canvas>();

            List<Screen> screens = new List<Screen>();
            foreach (var prefab in _data.screenPrefabs)
            {
                var screen = Instantiate(prefab, _bodyRoot);
                SetScreenVisibility(screen, false);
                screens.Add(screen);
            }
            
            foreach (CustomDebugger customDebugger in DebugCustomUtility.GetAllCustomDebugger()) {
                var customDebuggerScreen = Instantiate(_customDebuggerScreen, _bodyRoot);
                customDebuggerScreen.Initialize(customDebugger);
                SetScreenVisibility(customDebuggerScreen, false);
                screens.Add(customDebuggerScreen);
            }
            
            var home = Instantiate(_homePrefab, _bodyRoot);
            home.Initialize(screens);
            Push(home);

            Close();
        }

        public void Open()
        {
            AccessProcess.SetTrustedUser(true);
            AccessProcess.SetAccess(true);
            _canvas.enabled = true;
            _bodyRoot.gameObject.SetActive(true);
        }

        public void Push(Screen screen) 
        {
            if (_screenQueue.Count > 0)
            {
                var currentScreen = _screenQueue.Peek();
                if (currentScreen.mainSubScreen == null) 
                {
                    SetScreenVisibility(currentScreen, false);            
                }
            }

            SetScreenVisibility(screen, true);
            UpdateHeader(screen);
            
            _screenQueue.Push(screen);
            if (screen.mainSubScreen) 
            {
                Push(screen.mainSubScreen);
            }

            screen.OnScreenShow();
        }

        public Screen Pop()
        {
            if (IsHome)
            {
                Debug.LogError("Trying to dequeue when screen queue is only left one item should not be done.");
                return null;
            }

            var formerScreen = _screenQueue.Pop();
            SetScreenVisibility(formerScreen, false);
            
            var screen = _screenQueue.Peek();
            if (screen.mainSubScreen == formerScreen) 
            {
                return Pop();
            }

            SetScreenVisibility(screen, true);
            UpdateHeader(screen);

            return formerScreen;
        }

        public void Toggle(Screen screen)
        {
            Screen currentScreen = _screenQueue.Peek();
            if (currentScreen == screen) {
                Pop();
            } else {
                Push(screen);
            }
        }

        void SetScreenVisibility(Screen screen, bool visible) 
        {
            if (!visible)
            {
                screen.OnScreenHide();
            }
            screen.gameObject.SetActive(visible);

            Transform parent = visible ? _bodyRoot : _hiddenRoot;
            screen.transform.SetParent(parent, true);
        }

        public void UpdateHeader(Screen screen) => _header.UpdateTarget(screen);

        public void Close()
        {
            _canvas.enabled = false;
            _bodyRoot.gameObject.SetActive(false);
        }

        public void ShowPopup(DebuggerPopupConfig config)
        {
            Action closeCallback = null;
            
            // Standalone popup
            if (!IsOpened) {
                _header.gameObject.SetActive(false);
                _canvas.enabled = true;
                closeCallback = () => {
                    _header.gameObject.SetActive(true);
                    _canvas.enabled = false;
                };
            }
            
            _popupScreen.Show(config, closeCallback);
        }
    }
}
