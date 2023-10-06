using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Tune.Core;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneHomeDebugScreen : Screen
    {
        private const string TAG = "VoodooTuneHomeDebugScreen";

        [Header("Loading Screen"), SerializeField] private GameObject loadingRoot;
        [SerializeField] private GameObject normalDisplay;
        [SerializeField] private GameObject errorDisplay;
        [SerializeField] private Text errorLabel;
        [SerializeField] private Button retryButton;

        [Header("Home"), SerializeField] private GameObject homeRoot;
        [SerializeField] private GameObject headerRoot;
        
        [SerializeField] private Screen abTestScreen;
        [SerializeField] private Screen segmentsScreen;
        [SerializeField] private Screen sandboxScreen;
        
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

        private void Load()
        {
            loadingRoot.SetActive(true);
            normalDisplay.SetActive(true);
            errorDisplay.SetActive(false);
            headerRoot.SetActive(false);
            homeRoot.SetActive(false);
            
            DebugVTManager.LoadDebugConfigurations(OnSuccess, OnError);
        }

        private void OnSuccess()
        {
            OnInitDone();
            loadingRoot.SetActive(false);
            headerRoot.SetActive(true);
            homeRoot.SetActive(true);
        }

        private void OnError(string message)
        {
            normalDisplay.SetActive(false);
            errorDisplay.SetActive(true);
            errorLabel.text = message;
        }

        private void OnInitDone()
        {
            DebugVTManager.NewDebugDraftConfiguration();
            
            ClearDisplay();
            OpenFoldout("VoodooTune Debug");
            {
                var cohortButton = Button("A/B Tests", OnAbTestButtonClick);
                cohortButton.Interactable = DebugVTManager.GetDebugAbTests().Any();
                
                var segmentButton = Button("Segments", OnSegmentsButtonClick);
                segmentButton.Interactable = DebugVTManager.GetDebugSegments().Any();
                
                var sandboxButton = Button("Sandbox", OnSandboxButtonClick);
                sandboxButton.Interactable = DebugVTManager.GetDebugSandboxes().Any();
            }
            CloseFoldout();
            CreateInitRequest();
        }

        private void CreateInitRequest()
        {
            VoodooTuneInitAnalyticsInfo requestInfos = VoodooTuneManager.LastVoodooTuneInitInfo.infos;
            
            string resultType = VoodooTuneManager.LastVoodooTuneInitInfo.error ?? "Success";
            string httpResponse = requestInfos.HttpResponseCode + " - " + GetResponseName(requestInfos.HttpResponseCode);
            string cacheInfo = requestInfos.HasCache ? "Yes" : "No";
            
            OpenFoldout("Init Request");
            {
                Label("Result", resultType);
                Label("Http Response", httpResponse);
                Label("Duration", requestInfos.DurationInMilliseconds + "ms");
                Label("Timeout used", VoodooTuneManager.CurrentTimeoutInMilliseconds + "ms");
                Label("Has Cache", cacheInfo);
                InputField("New Timeout", "Enter Timeout...", OnTimeoutInputEndEdit);
                CopyToClipboard("Copy URL", VoodooTunePersistentData.SavedURL);
            }
            CloseFoldout();
        }

        private void OnAbTestButtonClick()
        {
            Debugger.Show(abTestScreen);
        }

        private void OnSegmentsButtonClick()
        {
            Debugger.Show(segmentsScreen);
        }

        private void OnSandboxButtonClick()
        {
            Debugger.Show(sandboxScreen);
        }

        private static void OnTimeoutInputEndEdit(string text)
        {
            if (string.IsNullOrEmpty(text)) {
                return;
            }

            try {
                DebugVTLocalTimeout.SetLocalTimeout(int.Parse(text));
            } catch (FormatException) {
                VoodooLog.LogError(Module.VOODOO_TUNE, TAG, "Can't convert " + text + " to a valid timeout.");
            }
        }

        private static string GetResponseName(double httpCode)
        {
            switch (httpCode) {
                case -1:
                    return "VoodooTune Timeout";
                case 0:
                    return "Couldn't reach server (bad url?)";
                case 200:
                    return "Success";
                case 204:
                    return "No configuration for this app";
                case 403:
                    return "Unauthorized";
                case 404:
                    return "Not found";
                case 422:
                    return "Unprocessable Entity";
            }

            return "";
        }
    }
}