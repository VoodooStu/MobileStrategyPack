using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Network;
using Voodoo.Tune.Core;
using Version = Voodoo.Tune.Core.Version;

namespace Voodoo.Tune.Debugger.Editor
{
	[Serializable]
	public class VTEDController
	{
		//VoodooTune system
		[NonSerialized] public VoodooTuneClient client;
		[NonSerialized] public string appId;
		[NonSerialized] public Version version;
		[NonSerialized] public IReadOnlyList<ABTest> abTests;
		[NonSerialized] public IReadOnlyList<Segment> segments;
		[NonSerialized] public IReadOnlyList<Layer> layers;
		[NonSerialized] public IReadOnlyList<ClassInfo> classInfos;
		[NonSerialized] public HealthCheck serverStatus;
		
		[NonSerialized] public WidgetManager widgetManager;
		
		private Platform _currentPlatform;
		public Platform CurrentPlatform
		{
			get => _currentPlatform;
			set
			{
				if (_currentPlatform == value)
				{
					return;
				}
				
				_currentPlatform = value;
				BundleId = null;
				
				ReloadAppInformation(true);
			}
		}

		private string _editorIDFA;
		public string EditorIDFA
		{
			get
			{
				if (_editorIDFA == null)
				{
					if (EditorPrefs.HasKey(VTEDConstants.EDITOR_PREF_EDITOR_IDFA))
					{
						_editorIDFA = EditorPrefs.GetString(VTEDConstants.EDITOR_PREF_EDITOR_IDFA);
					}
					else
					{
						_editorIDFA = Guid.NewGuid().ToString();
						EditorPrefs.SetString(VTEDConstants.EDITOR_PREF_EDITOR_IDFA, _editorIDFA);
					}
				}

				return _editorIDFA;
			}
		}

		private string _bundleId;
		public string BundleId
		{
			get
			{
				if (string.IsNullOrEmpty(_bundleId))
				{
					_bundleId = PlayerSettings.GetApplicationIdentifier(_currentPlatform == Platform.android ? BuildTargetGroup.Android : BuildTargetGroup.iOS);
				}
				
				return _bundleId;
			}
			private set
			{
				_bundleId = value;
				
				if (string.IsNullOrEmpty(_bundleId))
				{
					_bundleId = PlayerSettings.GetApplicationIdentifier(_currentPlatform == Platform.android ? BuildTargetGroup.Android : BuildTargetGroup.iOS);
				}
			}
		}

		public bool? InitSucceeded { get; private set; }
		public bool LiveVersionExist { get; private set; }

		public VTEDController(Platform platform)
		{
			_currentPlatform = platform;
			ReloadAppInformation(false);
		}

		public async void ReloadAppInformation(bool resetData)
		{
			Reset();
			client = new VoodooTuneClient();

			VTEDWindow.RepaintWindow();

			try
			{
				serverStatus = await client.ServerStatus.Get();
			}
			catch
			{
				return;
			}
			
			if (serverStatus.IsAlive == false)
			{
				InitSucceeded = false;
				VTEDWindow.RepaintWindow();
				return;
			}
			
			VTEDWindow.RepaintWindow();

			try
			{
				InitSucceeded = await GetVersion();
			}
			catch
			{
				return;
			}

			if (InitSucceeded == false)
			{
				VTEDWindow.RepaintWindow();
				return;
			}

			if (resetData || widgetManager == null)
			{
				widgetManager = new WidgetManager(abTests, layers, segments, classInfos, CurrentPlatform);
			}
			
			UpdateConfigurationEditor();
		}

		private void Reset()
		{
			WebRequest.CancelAllTasks();
			_bundleId = null;
			serverStatus = null;
			appId = null;
			version = null;
			abTests = null;
			segments = null;
			layers = null;
			classInfos = null;
			
			InitSucceeded = null;
			LiveVersionExist = false;
		}
		
		private async Task<bool> GetVersion()
		{
			IReadOnlyList<Version> versions = await client.App.Version.GetAll(BundleId);
				
			if (versions == null)
			{
				return false;
			}

			LiveVersionExist = versions.Count > 1;
			version = versions.FirstOrDefault(x => x.Status == VoodooTunePersistentData.SavedStatus);

			if (version == null)
			{
				abTests = new List<ABTest>();
				segments = new List<Segment>();
				layers = new List<Layer>();
				classInfos = new List<ClassInfo>();
				return false;
			}

			appId = version.AppId;
				
			VTEDWindow.RepaintWindow();
			abTests = await client.App.Version.AbTest.GetAll(appId, version.Id, new List<ABTestState> { ABTestState.draft, ABTestState.pending, ABTestState.published, ABTestState.terminating });
			segments = await client.App.Version.Segment.GetAll(appId, version.Id);
			layers = await client.App.Version.Layer.GetAll(appId, version.Id);
			classInfos = await client.App.Version.Class.GetAll(appId, version.Id);

			VTEDWindow.RepaintWindow();

			return true;
		}
		
		public async void UpdateConfigurationEditor()
		{
			VoodooTunePersistentData.SavedAppId = appId;

			await widgetManager.UpdateConfigurationEditor(client.CurrentServer, appId, BundleId, CurrentPlatform.ToString(), version);
			
			VTEDWindow.RepaintWindow();
		}
		
		public void ResetConfig()
		{
			VoodooTunePersistentData.Clear();

			UpdateConfigurationEditor();
			VTEDWindow.ShowToast(new GUIContent("Reset successful"));
		}

		public void ApplyCohortAndSegments()
		{
			VoodooTunePersistentData.Clear();
			
			string message = widgetManager.Save();

			if (string.IsNullOrEmpty(message)) //The UI is preventing this case to happen
			{
				return;
			}
			
			VoodooTunePersistentData.SavedVersionId = version.Id;
			UpdateConfigurationEditor();
			VTEDWindow.ShowToast(new GUIContent(message));
		}
		
		public async void RestartPlayMode()
		{
			if (EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
				while (EditorApplication.isPlaying)
				{
					await Task.Yield();
				}
			}

			EditorApplication.isPlaying = true;
		}
	}
}