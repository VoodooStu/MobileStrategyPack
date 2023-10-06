using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.IntegrationCheck.Editor
{
    /// <summary>
    /// Window For logging build errors and warnings. To use it, please check IntegrationCheckManager class instead of
    /// calling this class directly
    /// </summary>
    public class IntegrationCheckWindow : EditorWindow
    {
        private const string TITLE = "Integration Check";
        // GUI Related
        private Vector2 _scrollPos0;
        private Vector2 _scrollPos1;

        // Singleton - to preserve data between reloads/builds 
        private static IntegrationCheckWindow _instance;
        /// <summary>
        /// Get existing open window or if none, make a new one.
        /// </summary>
        private static IntegrationCheckWindow Instance {
            get {
                if (_instance == null) {
                    _instance = (IntegrationCheckWindow) GetWindow(typeof(IntegrationCheckWindow), true, TITLE, true);
                }

                return _instance;
            }
        }

        /* ==========================================================================================
         * Display Logic 
         * ==========================================================================================
         */

        private void OpenVSSetting()
        {
            var settings = Resources.Load<VoodooSettings>("VoodooSettings");
            AssetDatabase.OpenAsset(settings);
        }

        private IEnumerator StartHighlight(string settingName)
        {
            yield return new WaitForSeconds(.5f);
            Highlighter.Highlight("Inspector", settingName);
        }

        private void DisplayBackImage()
        {
            var backImage = (Texture) AssetDatabase.LoadAssetAtPath("Assets/VoodooSauce/IntegrationCheck/Editor/GoTo.png", typeof(Texture));
            var backGUI = new GUIContent(backImage);

            if (GUILayout.Button(backGUI, GUILayout.Width(30), GUILayout.Height(30))) {
                OpenVSSetting();
                GUIUtility.ExitGUI();
            }
        }

        private void DisplayError(string errorMessage, bool displayBackToSettingsBtn = false)
        {
            EditorGUILayout.BeginHorizontal();

            if (displayBackToSettingsBtn) {
                DisplayBackImage();
            }

            EditorGUILayout.HelpBox(
                errorMessage,
                MessageType.Error
            );
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayWarning(string warningMessage, bool displayBackToSettingsBtn = false)
        {
            EditorGUILayout.BeginHorizontal();

            if (displayBackToSettingsBtn) {
                DisplayBackImage();
            }

            EditorGUILayout.HelpBox(
                warningMessage,
                MessageType.Warning
            );
            EditorGUILayout.EndHorizontal();
        }

        internal static void ShowWindow()
        {
            Instance.Show();
        }
        
        void OnGUI()
        {
            List<IntegrationCheckMessage> errorMessages = IntegrationCheckManager.ErrorMessages;
            List<IntegrationCheckMessage> warningMessages = IntegrationCheckManager.WarningMessages;

            if (GUILayout.Button("Clear")) {
                IntegrationCheckManager.Clear();
            }

            if (IntegrationCheckManager.ErrorMessages.Count == 0 && warningMessages.Count == 0) {
                GUILayout.Label("No VoodooSauce Integration Errors Detected");
            }

            if (errorMessages.Count > 0) {
                GUILayout.Label($"VoodooSauce Integration Errors ({errorMessages.Count})", EditorStyles.boldLabel);
                GUILayout.Label("Please resolve the following.  Even if your Unity Build completed it is likely that it is incomplete or corrupted",
                    EditorStyles.miniLabel);

                _scrollPos0 = EditorGUILayout.BeginScrollView(_scrollPos0, new[] {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});

                for (var i = 0; i < errorMessages.Count; i++) {
                    DisplayError(errorMessages[i].Description, errorMessages[i].isBackToSettingsBtnDisplayed);
                }

                EditorGUILayout.EndScrollView();
            }

            if (warningMessages.Count > 0) {
                GUILayout.Label($"VoodooSauce Integration Warnings ({warningMessages.Count})", EditorStyles.boldLabel);

                _scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1, new[] {GUILayout.ExpandWidth(true), GUILayout.ExpandWidth(true)});

                for (var i = 0; i < warningMessages.Count; i++) {
                    DisplayWarning(warningMessages[i].Description, warningMessages[i].isBackToSettingsBtnDisplayed);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}