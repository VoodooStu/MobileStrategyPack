using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.VoodooTune;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneLoadingDebugScreen : Screen
    {
        [Header("Loading Screen"), SerializeField] private GameObject normalDisplay;
        [SerializeField] private GameObject errorDisplay;
        [SerializeField] private Text errorLabel;
        [SerializeField] private Screen screenToShow;
        [SerializeField] private GameObject commonElements;
        [SerializeField] private Button retryButton;

        private bool _initialised;
        private string _errorMessage;

        private void Start()
        {
            Load();
        }

        private void OnEnable()
        {
            retryButton.onClick.AddListener(Load);
        }

        private void OnDisable()
        {
            retryButton.onClick.RemoveListener(Load);
        }

        public override void OnScreenShow()
        {
            normalDisplay.SetActive(true);
            errorDisplay.SetActive(false);
            commonElements.SetActive(false);
            
            if (_initialised) {
                if (string.IsNullOrEmpty(_errorMessage)) {
                    Debugger.Show(screenToShow);
                } else {
                    OnError(_errorMessage);
                }
            }
        }

        private void Load()
        {
            _initialised = false;
            normalDisplay.SetActive(true);
            errorDisplay.SetActive(false);
            commonElements.SetActive(false);
            DebugVTManager.LoadDebugConfigurations(OnSuccess, OnError);
        }

        private void OnSuccess()
        {
            _initialised = true;
            
            _errorMessage = null;
            commonElements.SetActive(true);
        }

        private void OnError(string message)
        {
            _initialised = true;
            
            normalDisplay.SetActive(false);
            errorDisplay.SetActive(true);
            errorLabel.text = message;
            _errorMessage = message;
        }
    }
}
