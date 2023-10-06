using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.VoodooTune;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneConfigurationHeader : MonoBehaviour
    {
        [Header("Configuration"), SerializeField]
        private Text configId;
        [SerializeField]
        private Text configTitle;
        [SerializeField]
        private Text configurationDescriptionText;
        [SerializeField]
        private Color appliedConfigurationColor;
        [SerializeField]
        private Color draftConfigurationColor;
        [SerializeField]
        private GameObject warningRoot;
        [SerializeField]
        private GameObject currentConfigurationButtonsRoot;
        [SerializeField]
        private bool hasHiddenButton;

        [SerializeField]
        private Button detailsButton;
        [SerializeField]
        private Button resetButton;

        [SerializeField]
        private Screen detailsScreen;

        private Image _configurationBackground;

        private void Awake()
        {
            _configurationBackground = GetComponent<Image>();
        }

        private void Start()
        {
            DebugVTManager.OnDebugConfigurationChangeEvent += UpdateScreen;
            UpdateScreen();
        }

        private void OnDestroy()
        {
            DebugVTManager.OnDebugConfigurationChangeEvent -= UpdateScreen;
        }

        private void OnEnable()
        {
            detailsButton.onClick.AddListener(OnDetailsButtonClick);
            resetButton.onClick.AddListener(OnResetConfigurationBtn);
        }

        private void OnDisable()
        {
            detailsButton.onClick.RemoveListener(OnDetailsButtonClick);
            resetButton.onClick.RemoveListener(OnResetConfigurationBtn);
        }

        public void OnDetailsButtonClick()
        {
            Debugger.Toggle(detailsScreen);
        }

        public void OnResetConfigurationBtn()
        {
            DebugVTManager.ResetDebugDraftConfiguration();
        }

        private void UpdateScreen()
        {
            DebugVTConfiguration configurationToDisplay = DebugVTManager.IsDraftDebugConfiguration
                ? DebugVTManager.DraftDebugConfiguration
                : DebugVTManager.CurrentDebugConfiguration;

            string configurationId = VoodooTuneManager.GetConfigurationId();

            configId.text = string.IsNullOrEmpty(configurationId) ? "No Voodoo Config Id" : $"Voodoo Conf Id: {configurationId}";
            configTitle.text = DebugVTManager.IsDraftDebugConfiguration ? "Draft configuration" : "Current configuration";
            _configurationBackground.color = DebugVTManager.IsDraftDebugConfiguration ? draftConfigurationColor : appliedConfigurationColor;
            
            warningRoot.gameObject.SetActive(DebugVTManager.IsDraftDebugConfiguration && configurationToDisplay.IsValid);
            currentConfigurationButtonsRoot.SetActive(!DebugVTManager.IsDraftDebugConfiguration && !hasHiddenButton);
            resetButton.gameObject.SetActive(!DebugVTManager.IsDraftDebugConfiguration && !configurationToDisplay.IsDefaultConfiguration);

            configurationDescriptionText.text = "";
            if (configurationToDisplay.IsDefaultConfiguration) {
                configurationDescriptionText.text = "Normal configuration\n(No debug configuration is being applied)";
            } else if (configurationToDisplay.IsSandbox) {
                configurationDescriptionText.text = "Sandbox:\n" + configurationToDisplay.SelectedSandbox.Name;
            } else if (configurationToDisplay.IsSimulation) {
                if (configurationToDisplay.SelectedCohorts.Count > 0) {
                    configurationDescriptionText.text =
                        $"A/B tests ({configurationToDisplay.SelectedCohorts.Count}): {configurationToDisplay.SelectedABTestsAndCohortsToString}";
                }

                if (configurationToDisplay.SelectedSegments.Count > 0) {
                    configurationDescriptionText.text +=
                        $"\nSegments ({configurationToDisplay.SelectedSegments.Count}): {configurationToDisplay.SelectedSegmentsToString}";
                }
            }
        }
    }
}