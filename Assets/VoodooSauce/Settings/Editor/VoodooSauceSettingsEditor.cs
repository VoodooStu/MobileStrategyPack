#define VOODOO_SAUCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Core.Model;
using Voodoo.Sauce.Internal.IntegrationCheck;
using Voodoo.Sauce.Internal.IntegrationCheck.Editor;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;
using Voodoo.Sauce.Privacy;

// ReSharper disable  UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    [CustomEditor(typeof(VoodooSettings))]
    public class VoodooSauceSettingsEditor : UnityEditor.Editor
    {
#region Constants

        private const string VOODOO_SAUCE_SYMBOL = "VOODOO_SAUCE";
        private const string NO_VOODOO_SETTINGS_ERROR_MESSAGE =
            "No VoodooSettings file found. Check your path for Assets/Resources/VoodooSettings.asset";
        private const double REFRESH_BUTTON_COLOR_FEEDBACK_TIME = 4;
        private const double COPY_BUTTON_FEEDBACK_TIME = 1;

        private const float HEADER_IMAGE_WIDTH_PIXELS = 586;
        private const float HEADER_IMAGE_HEIGHT_PIXELS = 150;

#endregion

#region Properties

        private UnityWebRequest _www;

        private VoodooSettings Settings => target as VoodooSettings;

        private static KitchenSettingsJSON _kitchenSettings;
        private static string[] _stores;

        // if this value is equal to -1, that means that there is no store selected or the selected store is not in the list anymore.
        private static int _selectedStoreIndex = -1;

        private static string SelectedStore => _stores != null && _selectedStoreIndex >= 0 && _selectedStoreIndex < _stores.Length
            ? _stores[_selectedStoreIndex]
            : "";

        private static bool HasSelectedStore => !string.IsNullOrEmpty(SelectedStore);
        private static bool AreStoresLoaded => _stores != null && _stores.Length > 0;

        private static string _editorIdfa;

#endregion

#region Menu

        [MenuItem("VoodooSauce/Edit Settings")]
        private static void EditSettings()
        {
            VoodooSettings settings = VoodooSettings.Load();
            if (settings == null) {
                Debug.LogWarning("VoodooSettings can't be found, creating a new one...");
                settings = CreateInstance<VoodooSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Resources/VoodooSettings.asset");
                settings = VoodooSettings.Load();
            }

            Selection.activeObject = settings;
        }

        public enum IntegrationCheckResult
        {
            SUCCESS,
            ERROR,
            WARNING
        }

        /// <summary>
        /// Runs all checks from IntegrationCheck classes.
        /// This method is called directly from the VoodooSettings editor interface and automatically before each build.
        /// The main purpose is to shows warning and error messages for game developers to help them to integrate the VoodooSauce package.
        /// </summary>
        /// <returns>Returns false if the VoodooSettings file is not present in the project, true otherwise.</returns>
        /// 
        [MenuItem("VoodooSauce/Integration Check")]
        public static IntegrationCheckResult IntegrationCheck()
        {
            AddVoodooSauceDefineSymbol();
            // Update the Build Error Window 
            Console.Clear();
            IntegrationCheckManager.Clear();
            var settings = Resources.Load<VoodooSettings>(VoodooSettings.NAME);
            if (settings == null) {
                IntegrationCheckManager.LogMessage(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, NO_VOODOO_SETTINGS_ERROR_MESSAGE));
                Debug.Log("[VoodooSauce] No VoodooSettings found.  Ending Verification process.");
                return IntegrationCheckResult.ERROR;
            }

            // End of out of scope calls.
            var integrationCheckMessages = new List<IntegrationCheckMessage>();
            var coreIntegrationMessages = VoodooSauceCore.IntegrationCheck(settings);
            if (coreIntegrationMessages != null && coreIntegrationMessages.Count > 0) {
                integrationCheckMessages.AddRange(coreIntegrationMessages);
            }

            foreach (IIntegrationCheck integrationCheckInterface in AssembliesUtils.InstantiateInterfaceImplementations<IIntegrationCheck>()) {
                var messages = integrationCheckInterface.IntegrationCheck(settings);
                if (messages != null && messages.Count > 0) {
                    integrationCheckMessages.AddRange(messages);
                }
            }

            var hasError = false;
            foreach (IntegrationCheckMessage message in integrationCheckMessages) {
                if (message.type == IntegrationCheckMessage.Type.ERROR) hasError = true;
                IntegrationCheckManager.LogMessage(message);
            }

            Debug.Log("[VoodooSauce] Verification has been done.");
            if (hasError) return IntegrationCheckResult.ERROR;
            if (integrationCheckMessages.Count > 0) return IntegrationCheckResult.WARNING;
            return IntegrationCheckResult.SUCCESS;
        }

#endregion

#region Methods

        private static IEnumerator IntegrationCheckAsync(Action<IntegrationCheckResult> onCompleteCallback = null)
        {
            yield return null;
            IntegrationCheckResult result = IntegrationCheck();
            onCompleteCallback?.Invoke(result);
        }

        private static void AddVoodooSauceDefineSymbol()
        {
            foreach (BuildTargetGroup group in new[] {BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone}) {
                AppendScriptingDefineSymbolsForGroup(group);
            }
        }

        private static void AppendScriptingDefineSymbolsForGroup(BuildTargetGroup group)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (definesString.Contains(VOODOO_SAUCE_SYMBOL)) {
                return;
            }

            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.Add(VOODOO_SAUCE_SYMBOL);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(@group, string.Join(";", allDefines.ToArray()));
        }

        private static void LoadStoreSettings()
        {
            _kitchenSettings = KitchenSettingsJSON.Load();
            _stores = _kitchenSettings.GetStores().ToArray();
            _selectedStoreIndex = -1;

            VoodooSettings voodooSettings = VoodooSettings.Load();
            for (var i = 0; i < _stores.Length; i++) {
                if (_stores[i].Equals(voodooSettings.Store)) {
                    _selectedStoreIndex = i;
                    break;
                }
            }
        }

        private static void LoadEditorIdfa()
        {
            _editorIdfa = EditorIdfa.Get();
        }

#endregion

#region Lifecycle

        private void OnEnable()
        {
            LoadEditorIdfa();
            LoadStoreSettings();
            EditorApplication.update += OnUpdate;
        }

        private void OnUpdate()
        {
            if (_requestRepaint && EditorApplication.timeSinceStartup >= _refreshButtonCustomDeathTime) {
                // The inspector needs to be repainted because the "Refresh Settings" button graphic has changed.
                _requestRepaint = false;
                Repaint();
            }

            if (_requestRepaint && EditorApplication.timeSinceStartup >= _integrationCheckButtonCustomDeathTime) {
                _requestRepaint = false;
                Repaint();
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        protected override void OnHeaderGUI()
        {
            // Empty to hide the default header
        }

        public override void OnInspectorGUI()
        {
            DrawHeaderUI();
            DrawVoodooSauceVersionUI();
            DrawStatusUI();
            DrawStoresUI();
            DrawIntegrationCheckUI();
            DrawRefreshSettingsUI();
            DrawEditorIdfaUI();

            base.OnInspectorGUI();
        }

#endregion

#region Draw UI

        private static void DrawHeaderUI()
        {
            Rect rect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();

            float x = (rect.width - HEADER_IMAGE_WIDTH_PIXELS) / 2;
            if (x < 0) {
                x = 0;
            }

            float y = (rect.height - HEADER_IMAGE_HEIGHT_PIXELS) / 2;
            if (y < 0) {
                y = 0;
            }

            var image = (Texture) AssetDatabase.LoadAssetAtPath("Assets/VoodooSauce/Settings/Editor/Images/VoodooSettings.png", typeof(Texture));
            EditorGUI.DrawPreviewTexture(new Rect(x, y, HEADER_IMAGE_WIDTH_PIXELS, HEADER_IMAGE_HEIGHT_PIXELS), image);
            GUILayout.Space(HEADER_IMAGE_HEIGHT_PIXELS);
            DrawSmallSpacerUI();
        }

        private static void DrawVoodooSauceVersionUI()
        {
            DrawSmallSpacerUI();
            EditorGUILayout.LabelField("VoodooSauce SDK Version", VoodooVersion.Load().ToString());
            DrawSmallSpacerUI();
        }

        private double _editorIdfaButtonCustomDeathTime;

        private void DrawEditorIdfaUI()
        {
            DrawSeparatorUI();

            EditorGUILayout.LabelField("Editor Idfa", new GUIStyle(GUI.skin.label) {
                fontStyle = FontStyle.Bold
            });

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_editorIdfa, new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleLeft,
            });

            bool previousGuiEnabled = GUI.enabled;
            var buttonText = "Copy";
            bool isEditorIdfaButtonCustom = _editorIdfaButtonCustomDeathTime - EditorApplication.timeSinceStartup > 0;

            if (isEditorIdfaButtonCustom) {
                GUI.enabled = false;
                buttonText = "Copied!";
            }

            if (GUILayout.Button(buttonText, GUILayout.Width(75))) {
                _editorIdfa.CopyToClipboard();
                _editorIdfaButtonCustomDeathTime = EditorApplication.timeSinceStartup + COPY_BUTTON_FEEDBACK_TIME;
                _requestRepaint = true;
            }

            if (isEditorIdfaButtonCustom) {
                GUI.enabled = previousGuiEnabled;
            }

            EditorGUILayout.EndHorizontal();
            DrawExplanationTextUI(
                "This IDFA is used by VoodooTune, Voodoo Analytics and the Privacy module in the editor mode. This ID is unique to every developer.");

            DrawSeparatorUI();
        }

        private void DrawStatusUI()
        {
            string store = SelectedStore;

            if (AreStoresLoaded) {
                LargeHeaderDrawer.Draw("Current store settings loaded", store);
            } else {
                EditorGUILayout.HelpBox(
                    "The VoodooSauce Settings are not loaded. Please click the button 'Refresh VoodooSauce Settings' to load them.",
                    MessageType.Error);
            }

            // The access token must be provided to download the settings from the Kitchen server.
            if (string.IsNullOrEmpty(Settings.AccessTokenID)) {
                EditorGUILayout.HelpBox(
                    "The Voodoo Server Access Token is missing. Set the Voodoo Server Access Token below (provided by Voodoo contact), then click 'Refresh VoodooSauce Settings'. This will auto populate the variables in your VoodooSettings - Automatic Settings section below",
                    MessageType.Error);
            }

            // Warn the game developer if this store is not compatible with some platforms.
            if (HasSelectedStore) {
                if (string.IsNullOrEmpty(Settings.IOSBundleID)) {
                    EditorGUILayout.HelpBox(
                        $"The iOS platform is not compatible with the store '{store}'. The iOS application could not be built.",
                        MessageType.Warning);
                }

                if (string.IsNullOrEmpty(Settings.AndroidBundleID)) {
                    EditorGUILayout.HelpBox(
                        $"The Android platform is not compatible with the store '{store}'. The Android application could not be built.",
                        MessageType.Warning);
                }
            }
        }

        private static void DrawStoresUI()
        {
            if (AreStoresLoaded) {
                DrawSmallSpacerUI();

                EditorGUI.BeginChangeCheck();

                _selectedStoreIndex = EditorGUILayout.Popup("Load Settings from Store", _selectedStoreIndex, _stores);

                if (EditorGUI.EndChangeCheck()) {
                    try {
                        VoodooSauceSettingsHelper.LoadSettings(SelectedStore);
                    } catch (StoreNotSupportedException e) {
                        UnityEditor.EditorUtility.DisplayDialog(e.Message, "Please choose another store", "Close");
                    }
                }
            }

            DrawSeparatorUI();
        }

        private Color? _integrationCheckButtonCustomColor;
        private string _integrationCheckButtonCustomText;
        private double _integrationCheckButtonCustomDeathTime;
        private bool _integrationCheckRequestRepaint;

        private void DrawIntegrationCheckUI()
        {
            if (!AreStoresLoaded) {
                return;
            }

            var previousGuiColor = GUI.color;
            var previousGuiEnabled = GUI.enabled;
            var buttonText = "Run Integration Test";
            var isIntegrationCheckButtonCustom = _integrationCheckButtonCustomColor != null
                && _integrationCheckButtonCustomDeathTime - EditorApplication.timeSinceStartup > 0;

            if (isIntegrationCheckButtonCustom) {
                // The refresh button has a custom color.
                GUI.enabled = false;
                GUI.color = _integrationCheckButtonCustomColor.Value;
                if (_integrationCheckButtonCustomText != null) {
                    // The refresh button also has a custom text.
                    buttonText = _integrationCheckButtonCustomText;
                }
            }

            if (GUILayout.Button(Environment.NewLine + buttonText + Environment.NewLine)) {
                // Run the integration test is only pertinent for supported configuration store/platform.
                BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
                if (!VoodooSauceSettingsHelper.IsCurrentStoreSupported(platform)) {
                    UnityEditor.EditorUtility.DisplayDialog($"The {platform} application could not be built.",
                        $"The {platform} platform is not compatible with the store '{SelectedStore}'",
                        "Close");
                    return;
                }

                _integrationCheckButtonCustomDeathTime = double.MaxValue;
                _integrationCheckButtonCustomText = "Doing Integration Check";
                EditorUtility.StartBackgroundTask(IntegrationCheckAsync((result) => {
                    switch (result) {
                        case IntegrationCheckResult.SUCCESS:
                            _integrationCheckButtonCustomColor = Color.green;
                            _integrationCheckButtonCustomText = "No Integration Error / Warnings";
                            break;
                        case IntegrationCheckResult.ERROR:
                            _integrationCheckButtonCustomColor = Color.red;
                            _integrationCheckButtonCustomText = "Errors in Integration Check";
                            break;
                        case IntegrationCheckResult.WARNING:
                            _integrationCheckButtonCustomColor = Color.yellow;
                            _integrationCheckButtonCustomText = "Warnings in Integration Check";
                            break;
                    }

                    _requestRepaint = true;
                    _integrationCheckButtonCustomDeathTime = EditorApplication.timeSinceStartup + REFRESH_BUTTON_COLOR_FEEDBACK_TIME;
                }));
            }

            if (isIntegrationCheckButtonCustom) {
                GUI.enabled = previousGuiEnabled;
                GUI.color = previousGuiColor;
            }

            DrawExplanationTextUI("Running the integration test ensures that the VoodooSauce SDK is well integrated in your game.");
            DrawSeparatorUI();
        }

        private Color? _refreshButtonCustomColor;
        private string _refreshButtonCustomText;
        private double _refreshButtonCustomDeathTime;
        private bool _requestRepaint;

        private void DrawRefreshSettingsUI()
        {
            var previousGuiColor = GUI.color;
            var previousGuiEnabled = GUI.enabled;
            var buttonText = "Refresh VoodooSauce Settings";
            var refreshButtonCustom = _refreshButtonCustomColor != null && _refreshButtonCustomDeathTime - EditorApplication.timeSinceStartup > 0;
            if (refreshButtonCustom) {
                // The refresh button has a custom color.
                GUI.enabled = false;
                GUI.color = _refreshButtonCustomColor.Value;
                if (_refreshButtonCustomText != null) {
                    // The refresh button also has a custom text.
                    buttonText = _refreshButtonCustomText;
                }
            }

            if (GUILayout.Button(Environment.NewLine + buttonText + Environment.NewLine)) {
                _refreshButtonCustomColor = Color.yellow;
                _refreshButtonCustomDeathTime = double.MaxValue;
                _refreshButtonCustomText = null;
                EditorUtility.StartBackgroundTask(VoodooSauceSettingsManager.RefreshVoodooSauceSettings(true, (success, message) => {
                    if (success) {
                        _refreshButtonCustomColor = Color.green;
                        _refreshButtonCustomText = "Correctly refreshed.";
                    } else {
                        _refreshButtonCustomColor = Color.red;
                        _refreshButtonCustomText = "An error has happened.";
                    }

                    _requestRepaint = true;
                    _refreshButtonCustomDeathTime = EditorApplication.timeSinceStartup + REFRESH_BUTTON_COLOR_FEEDBACK_TIME;
                }), LoadStoreSettings);
            }

            if (refreshButtonCustom) {
                GUI.enabled = previousGuiEnabled;
                GUI.color = previousGuiColor;
            }

            DrawSmallSpacerUI();
        }

#endregion

#region Draw items

        private static void DrawExplanationTextUI(string text)
        {
            ExplanationTextDrawer.Draw(text);
        }

        private static void DrawSeparatorUI()
        {
            SeparatorDrawer.Draw();
        }

        private static void DrawSmallSpacerUI()
        {
            SmallSpacerDrawer.Draw();
        }

#endregion
    }
}