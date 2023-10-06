using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Editor
{
    /// <summary>
    /// Displays all changed and unchanged values during the sync between local VoodooSettings and VoodooServer's info. 
    /// </summary>
    public class VoodooSettingsChangedWindow : EditorWindow
    {
        private const string TITLE = "VoodooSettingsChangedWindow";

        private Vector2 _scrollPos;
        
        private static VoodooSettingsChangedWindow _instance;
        private static VoodooSettingsChangedWindow Instance {
            get {
                if (_instance == null) {
                    _instance = (VoodooSettingsChangedWindow) GetWindow(typeof(VoodooSettingsChangedWindow), true, TITLE, true);
                }

                return _instance;
            }
        }

        private List<KeyValuePair<string, KeyValuePair<string, string>>> _changedValuesList;
        private string _timestamp;

        public static void ShowWindow(string timestamp,
                                      List<KeyValuePair<string, KeyValuePair<string, string>>> changedValuesList)
        {
            // Get existing open window or if none, make a new one:
            VoodooSettingsChangedWindow window = Instance;
            window._changedValuesList = changedValuesList;
            window._timestamp = timestamp;
            window.position = new Rect(0, 0, 600, 800);
            window.Show();
        }

        private void OnGUI()
        {
            if (_changedValuesList == null) {
                Close();
                return;
            }
            
            LargeHeaderDrawer.Draw(
                "VoodooSauceSettings updated remotely",
                "VoodooSauceSettings Update Result"
            );

            // General Info 
            var generalInfoStyle = new GUIStyle {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal = {textColor = Color.white},
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Updated at: " + _timestamp, generalInfoStyle);

            if (_changedValuesList.Count == 0) {
                generalInfoStyle.normal.textColor = Color.green;
                GUILayout.Label("You are in sync with all VoodooSettings on VoodooServer", generalInfoStyle);
            }

            GUILayout.Space(25);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            // === CHANGED VALUES === (values downloaded from VoodooServer that override local VoodooSettings) 
            if (_changedValuesList.Count > 0) {
                LargeHeaderDrawer.Draw("Fields updated from VoodooServer", "VoodooSettings DOWNLOADED");
            }
            
            foreach (KeyValuePair<string, KeyValuePair<string, string>> changedValue in _changedValuesList) {
                // TEMP Styling 

                var headerStyle = new GUIStyle {fontStyle = FontStyle.Bold, fontSize = 14, normal = {textColor = Color.white}, wordWrap = true};

                var subHeaderStyle = new GUIStyle {fontSize = 12, normal = {textColor = Color.white}, wordWrap = true};

                string fieldName = changedValue.Key;
                object oldFieldValue = changedValue.Value.Key;
                object newFieldValue = changedValue.Value.Value;

                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Space(10);
                GUILayout.Label(fieldName, headerStyle);

                GUILayout.Space(5);

                GUILayout.Label("OLD: " + oldFieldValue, subHeaderStyle);
                GUILayout.Label("NEW: " + newFieldValue, subHeaderStyle);

                GUILayout.Space(10);
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }
    }
}