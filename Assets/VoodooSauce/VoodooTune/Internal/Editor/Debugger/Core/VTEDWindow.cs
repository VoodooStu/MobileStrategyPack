using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Network;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class VTEDWindow : EditorWindow
	{
		//Window variables
		private VTEDController _controller;
		private int _platformIndex = 0;

		private bool _foldoutGeneralInfo = true;
		private bool _foldoutConfigurationSetup = true;
		private bool _foldoutConfigurationSimulator = true;
		private bool _expandAll = true;

		private GUIStyle _richLabelStyle;
		private GUIStyle _linkLabelStyle;
		private GUIStyle _richAndBoldLabelStyle;
		private GUIStyle _foldoutHeaderStyle;
		// private GUIStyle _foldoutHeaderWithButtonStyle;
		private List<GUIContent> icons = new List<GUIContent>();

		private static Action _askForRepaint;
		private static Action<GUIContent> _showNotification;

#if NEWTONSOFT_JSON
		[MenuItem("VoodooSauce/VoodooTune/Editor Debugger")]
		public static void InitConfig()
		{
			VTEDWindow window = GetWindow<VTEDWindow>("VoodooTune Editor Debugger", true);
			window.Show();
		}
#endif

		private void OnEnable()
		{
			_askForRepaint += Repaint;
			_showNotification += ShowNotification;
			
			_controller = new VTEDController((Platform)_platformIndex);
			
			icons = new List<GUIContent>
			{
				EditorGUIUtility.IconContent("d_BuildSettings.iPhone.Small"),
				EditorGUIUtility.IconContent("d_BuildSettings.Android.Small")
			};
		}

		private void OnDisable()
		{
			_askForRepaint -= Repaint;
			_showNotification -= ShowNotification;
			WebRequest.CancelAllTasks();
		}

		public static void RepaintWindow()
		{
			_askForRepaint?.Invoke();
		}

		public static void ShowToast(GUIContent content)
		{
			_showNotification?.Invoke(content);
		}

		private void OnGUI()
		{
			_richLabelStyle = _richLabelStyle ?? new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };

			_linkLabelStyle = _linkLabelStyle ?? new GUIStyle(EditorStyles.label)
			{
				richText = true,
				wordWrap = true,
				margin = EditorStyles.boldLabel.margin,
				padding = EditorStyles.boldLabel.padding,
				normal = {
					textColor = new Color(0.48f, 0.67f, 0.94f, 1f)
				},
				stretchWidth = false
			};
			
			_richAndBoldLabelStyle = _richAndBoldLabelStyle ?? new GUIStyle(EditorStyles.boldLabel)
			{
				richText = true,
				wordWrap = true
			};
			
			_foldoutHeaderStyle = _foldoutHeaderStyle ?? new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold};

			try
			{
				DisplayGeneralInformation();
			}
			catch
			{
				return;
			}

			DrawLine();

			if (_controller.InitSucceeded == null || _controller.serverStatus.IsAlive == false)
			{
				return;
			}
			
			if (_controller.appId == null)
			{
				DisplayAppIdMissingInfo();
				return;
			}
			
			DisplayConfigurationSetup();

			if (_controller.version == null)
			{
				return;
			}
			
			DrawLine();
			
			DisplayConfigurationSimulator();
		}

		private void DisplayPublishButton()
		{
			if (_controller.LiveVersionExist)
			{
				return;
			}
			
			if (GUILayout.Button("Publish a new version"))
			{
				PublishVersion();
			}
		}

		private async void PublishVersion()
		{
			EditorUtility.DisplayProgressBar("Publish version", "A new version is currently being published, please wait...", 0.33f);
			Repaint();
			VersionResponse versionResponse = await _controller.client.App.Version.Publish(_controller.appId, "First version");
			EditorUtility.ClearProgressBar();
			string message = "Version published successfully" +
			                 $"\nVersion Name : {versionResponse.Name}" +
			                 $"\nVersion Id  : {versionResponse.Id}";
			ShowNotification(new GUIContent(message));
		}

		private void DrawLine()
		{
			EditorGUILayout.Space();
			Rect rect = EditorGUILayout.GetControlRect(false, 1);
			rect.x += 10;
			rect.width -= 20;
			EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1));
			EditorGUILayout.Space();
		}

		private void DisplayGeneralInformation()
		{
			EditorGUILayout.BeginHorizontal();
			{
#if UNITY_2019_1_OR_NEWER
				_foldoutGeneralInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutGeneralInfo, "General information");
#else
				_foldoutGeneralInfo = EditorGUILayout.Foldout(_foldoutGeneralInfo, "General information", true, _foldoutHeaderStyle);
#endif

				bool isGUIEnabled = GUI.enabled;
				GUI.enabled = _controller.InitSucceeded != null;
				if (GUILayout.Button("Reload the app info", GUILayout.Width(150f)))
				{
					_controller.ReloadAppInformation(true);
				}

				GUI.enabled = isGUIEnabled;

				GUILayout.Space(VTEDConstants.rightMargin);
			}
			EditorGUILayout.EndHorizontal();

			if (_foldoutGeneralInfo == false)
			{
#if UNITY_2019_1_OR_NEWER
				EditorGUILayout.EndFoldoutHeaderGroup();
#endif
				return;
			}
			
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(VTEDConstants.leftMargin);
				EditorGUILayout.BeginVertical();
				{
					EditorGUIUtility.labelWidth = 100f;
					DisplayServerStatus();
					EditorGUILayout.Space();
					DisplayIds();
					EditorGUILayout.Space();
					DisplayEnvironment();
					EditorGUILayout.Space();
					DisplayPlatform();
					EditorGUIUtility.labelWidth = 150f;
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(VTEDConstants.rightMargin);
			}
			EditorGUILayout.EndHorizontal();

#if UNITY_2019_1_OR_NEWER
				EditorGUILayout.EndFoldoutHeaderGroup();
#endif
		}
		
		private void DisplayServerStatus()
		{
			string status = "Loading...";
			string version = "";

			if (_controller.serverStatus != null)
			{
				status = _controller.serverStatus.IsAlive ? $"<color={VTEDConstants.greenColor}>Normal</color>" : $"<color={VTEDConstants.redColor}>Not Responding</color>";
				version = _controller.serverStatus.IsAlive ? $"{_controller.serverStatus.Version}" : "-";
			}
			
			EditorGUILayout.LabelField("Server status", status, _richAndBoldLabelStyle);
			EditorGUILayout.LabelField("API version", version, _richAndBoldLabelStyle);
		}

		private void DisplayIds()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Editor IDFA");

				if (string.IsNullOrEmpty(_controller.EditorIDFA))
				{
					EditorGUILayout.LabelField("-", _richAndBoldLabelStyle);
				}
				else
				{
					if (GUILayout.Button(_controller.EditorIDFA, _linkLabelStyle))
					{
						EditorGUIUtility.systemCopyBuffer = _controller.EditorIDFA;
						ShowNotification(new GUIContent("Editor IDFA copied to clipboard"));
					}
					EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Bundle ID", _controller.BundleId, _richAndBoldLabelStyle);
			
			string appId;

			if (string.IsNullOrEmpty(_controller.appId) == false)
			{
				appId = _controller.appId;
			}
			else
			{
				appId = _controller.InitSucceeded == null ? "Loading..." : "-";
			}
			
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("App ID");

				if (string.IsNullOrEmpty(_controller.appId))
				{
					EditorGUILayout.LabelField(appId, _richAndBoldLabelStyle);
				}
				else
				{
					if (GUILayout.Button(appId, _linkLabelStyle))
					{
						EditorGUIUtility.systemCopyBuffer = appId;
						ShowNotification(new GUIContent("App Id copied to clipboard"));
					}

					EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DisplayEnvironment()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Environment");
				GUILayout.Space(1f);
				Server server = (Server)EditorGUILayout.Popup((int)VoodooTunePersistentData.SavedServer, VoodooTunePersistentData.SavedServerDisplayNames);
				
				if (server != VoodooTunePersistentData.SavedServer)
				{
					VoodooTunePersistentData.SavedServer = server;
					_controller.ReloadAppInformation(true);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Version");
				GUILayout.Space(1f);
				Status status = (Status)EditorGUILayout.Popup((int)VoodooTunePersistentData.SavedStatus, VoodooTunePersistentData.SavedStatusDisplayNames);
				
				if (status != VoodooTunePersistentData.SavedStatus)
				{
					VoodooTunePersistentData.SavedStatus = status;
					_controller.ReloadAppInformation(true);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DisplayPlatform()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Platform");
			GUILayout.Space(1f);
			
			if (_controller.widgetManager?.configurationWidget.Classes == null && string.IsNullOrEmpty(_controller.appId) == false)
			{
				GUI.enabled = false;
			}
			
			EditorGUI.BeginChangeCheck();
			_platformIndex = GUILayout.Toolbar(_platformIndex, icons.ToArray(), GUILayout.Width(150f));
			if (EditorGUI.EndChangeCheck())
			{
				_controller.CurrentPlatform = (Platform)_platformIndex;
			}
			
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
		}

		private void DisplayAppIdMissingInfo()
		{
			string message = $"There is no App with the bundle id {PlayerSettings.applicationIdentifier}.\n" +
			                 $"Please verify your bundle id for the {_controller.CurrentPlatform.ToString()} platform or go to the VoodooTune dashboard to add your app.";

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(VTEDConstants.leftMargin);
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox(message, MessageType.Error);
				GUI.backgroundColor = Color.white;
				GUILayout.Space(VTEDConstants.rightMargin);
			}
			EditorGUILayout.EndHorizontal();
				
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Go to VoodooTune dashboard"))
				{
					Application.OpenURL($"https://voodootune.voodoo-{VoodooTunePersistentData.SavedServer}.io/games/import");
				}

				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DisplayConfigurationSetup()
		{
#if UNITY_2019_1_OR_NEWER
			_foldoutConfigurationSetup = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutConfigurationSetup, "Configuration setup");
#else
			_foldoutConfigurationSetup = EditorGUILayout.Foldout(_foldoutConfigurationSetup, "Configuration setup", true, _foldoutHeaderStyle);
#endif
			
			if (_foldoutConfigurationSetup == false)
			{
#if UNITY_2019_1_OR_NEWER
				EditorGUILayout.EndFoldoutHeaderGroup();
#endif
				return;
			}

			if (_controller.version != null)
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Space(VTEDConstants.leftMargin);
					EditorGUILayout.BeginVertical();
					{
						_controller.widgetManager?.OnGUI();

						EditorGUILayout.Space();
						
						DisplayConfigurationButtons();
					}
					EditorGUILayout.EndVertical();
					GUILayout.Space(VTEDConstants.rightMargin);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				string message = $"There are no \"{VoodooTunePersistentData.SavedStatus}\" version for your app.";
				if (VoodooTunePersistentData.SavedStatus == Status.live)
				{
					message += " You need to publish a version of your app at least once before being able to access a live environment.";
				}
				else
				{
					message += " Please contact the support team for more information";
				}

				EditorGUILayout.HelpBox(message, MessageType.Error);
				EditorGUILayout.Space();

				if (VoodooTunePersistentData.SavedStatus == Status.live)
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Go to VoodooTune dashboard"))
						{
							Application.OpenURL($"https://voodootune.voodoo-{VoodooTunePersistentData.SavedServer}.io/games/{_controller.appId}/wip/publish");
						}

						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			
#if UNITY_2019_1_OR_NEWER
			EditorGUILayout.EndFoldoutHeaderGroup();
#endif
		}

		private void DisplayConfigurationButtons()
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (_controller.widgetManager.IsSimulating == false)
				{
					GUI.enabled = false;
				}
				
				if (GUILayout.Button("Reset Config"))
				{
					_controller.ResetConfig();
				}
				GUILayout.Space(8f);
				
				GUI.enabled = true;

				if (_controller.widgetManager.CanApply == false)
				{
					GUI.enabled = false;
				}

				if (GUILayout.Button("Apply and restart"))
				{
					_controller.ApplyCohortAndSegments();
					_controller.RestartPlayMode();
				}
				
				if (GUILayout.Button("Apply"))
				{
					_controller.ApplyCohortAndSegments();
				}

				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DisplayConfigurationSimulator()
		{
			EditorGUILayout.BeginHorizontal();
			{
#if UNITY_2019_1_OR_NEWER
				_foldoutConfigurationSimulator = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutConfigurationSimulator, "Configuration simulator");
#else
				_foldoutConfigurationSimulator = EditorGUILayout.Foldout(_foldoutConfigurationSimulator, "Configuration simulator", true, _foldoutHeaderStyle);
#endif

				EditorGUI.BeginChangeCheck();
				_expandAll = GUILayout.Toggle(_expandAll, "Expand All", GUILayout.Width(76f));
				if (EditorGUI.EndChangeCheck())
				{
					_controller.widgetManager.configurationWidget.ExpandAll(_expandAll);
				}

				GUILayout.Space(VTEDConstants.rightMargin);
			}
			EditorGUILayout.EndHorizontal();

			if (_foldoutConfigurationSimulator == false)
			{
#if UNITY_2019_1_OR_NEWER
				EditorGUILayout.EndFoldoutHeaderGroup();
#endif
				return;
			}

			_controller.widgetManager.configurationWidget?.OnGUI();
			
#if UNITY_2019_1_OR_NEWER
			EditorGUILayout.EndFoldoutHeaderGroup();
#endif
		}
	}
}