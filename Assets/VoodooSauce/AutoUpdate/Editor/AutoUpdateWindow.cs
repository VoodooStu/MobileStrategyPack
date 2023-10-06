using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    [InitializeOnLoad]
    public class NewVersionWindow : EditorWindow
    {
        private static Package _package;

        private const string EPrefLastSeenDate = "VS_lastSeenDate";
        private const int DelayTimeHours = 2;

        static NewVersionWindow()
        {
            EditorApplication.update += Startup;
        }

        static void Startup()
        {
            EditorApplication.update -= Startup;

            CheckPackageAvailable();
        }

        /// <summary>
        /// Get all variables set in the VoodooServer 
        /// </summary>
        private static void CheckPackageAvailable()
        {
            AutoUpdateManager.LoadAutoUpdate(delegate(Package package) {
                if (package != null) {
                    ShowWindow(package);
                }
            });
        }

        private static void ShowWindow(Package package)
        {
            // Check delay time has passed
            if (EditorPrefs.HasKey(EPrefLastSeenDate)) {
                DateTime lastSeenDate = DateTime.Parse(EditorPrefs.GetString(EPrefLastSeenDate));
                if (DateTime.Now < lastSeenDate.AddHours(DelayTimeHours)) {
                    return;
                }
            }

            EditorPrefs.SetString(EPrefLastSeenDate, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            _package = package;
            var window = (NewVersionWindow) GetWindow(typeof(NewVersionWindow), true, package.Title, true);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(20);

            if (_package.SubTitle != null) {
                EditorGUILayout.LabelField(_package.SubTitle, new GUIStyle {
                    fontStyle = FontStyle.Bold,
                    fontSize = 20,
                    normal = {textColor = Color.white},
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                });
                GUILayout.Space(5);
            }

            if (_package.WarningMessage != null) {
                EditorGUILayout.LabelField(_package.WarningMessage, new GUIStyle {
                    fontStyle = FontStyle.Bold,
                    fontSize = 13,
                    normal = {textColor = Color.red},
                    alignment = TextAnchor.MiddleCenter
                });
                GUILayout.Space(10);
            }

            EditorGUILayout.LabelField(_package.Message, new GUIStyle {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = Color.white},
                wordWrap = true,
            });

            GUILayout.Space(5);

            if (_package.UpdateInstructionsUrl != null) {
                if (GUILayout.Button("Update Instructions")) {
                    Application.OpenURL(_package.UpdateInstructionsUrl);
                }
            }

            if (GUILayout.Button("Download and Import package")) {
                Close();
                AutoUpdateManager.DownloadAndInstallPackage(_package);
            }

            GUILayout.EndVertical();
        }
    }
}