using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Core;
using Version = Voodoo.Tune.Core.Version;

namespace Voodoo.Tune.Debugger.Editor
{
	public class ConfigurationWidget
	{
		private readonly VoodooTuneConfigurationManager _configurationManager;
		
		private string _appId;
		private string _bundleId;
		private string _platform;
		private string _appVersion;
		private string _versionId;
		private List<Segment> _segments;
		private List<ABTest> _abTests;
		private List<Cohort> _cohorts;
		private bool _liveVersionExist;

		private bool _foldoutSelectionSetup = true;
		private bool _foldoutDisplayConfiguration = true;
		private Vector2 _scrollPosition;
		
		//UI
		private string _configStatus;
		private string _versionStatus;
		private string _segmentNames;
		private string _abTestNames;
		private string _cohortNames;
		
		private GUIStyle _richLabelStyle;
		private GUIStyle _foldoutHeaderStyle;
		private GUIStyle _linkLabelStyle;
		private GUIStyle _modifiedValueStyle;
		
		public List<ConfigurationClass> Classes { get; private set; }
		public string ConfigurationURL { get; private set; }

		public ConfigurationWidget(Server server, string appId, string bundleId, string platform, string appVersion, Version version)
		{
			_appId = appId;
			_bundleId = bundleId;
			_platform = platform;
			_appVersion = appVersion;
			_versionId = version.Id;
			_versionStatus = version.Status.ToString();
			_segments = VoodooTunePersistentData.SavedSegments;
			_abTests = VoodooTunePersistentData.SavedABTests;
			_cohorts = VoodooTunePersistentData.SavedCohorts;

			_liveVersionExist = version.HistorySeq > 1 || (version.HistorySeq == 1 && version.Status != Status.wip);
			
			_configStatus = _cohorts == null && (_segments == null || _segments.Count == 0) ? $"<color={VTEDConstants.greenColor}>Default value</color>" : $"<color={VTEDConstants.orangeColor}>Simulation</color>";
			_segmentNames = _segments != null && _segments.Count > 0 ? string.Join(", ", _segments.Select(x => x.Name)) : "-";
			_abTestNames = _abTests != null && _abTests.Count > 0 ? string.Join(", ", _abTests.Select(x => x.Name)) : "-";
			_cohortNames = _cohorts != null && _cohorts.Count > 0 ? string.Join(", ", _cohorts.Select(x => x.Name)) : "-";

			_configurationManager = new VoodooTuneConfigurationManager(server);
			SetConfigurationURL();
		}

		private void SetConfigurationURL()
		{
			if (_cohorts?.Count > 0 || _segments?.Count > 0)
			{
				ConfigurationURL = _configurationManager.CreateSimulationURL(_appId, _platform, _appVersion, _versionId, _segments?.Select(x => x.Id).ToArray(), _abTests?.Select(x => x.Id).ToArray(), _cohorts?.Select(x => x.Id).ToArray());
			}
			else
			{
				ConfigurationURL = _configurationManager.CreateDefaultConfigurationURL(VoodooSauceVariables.GetVSParams(), _bundleId, _platform, _appVersion);
			}
		}

		public async Task Simulate(IReadOnlyList<ClassInfo> classInfos)
		{
			Dictionary<Type, List<object>> configs = await _configurationManager.LoadAndParseConfigurationAsync(ConfigurationURL);
			
			List<ConfigurationClass> newClasses = new List<ConfigurationClass>();
			foreach (KeyValuePair<Type, List<object>> kvp in configs)
			{
				ConfigurationClass newClass = new ConfigurationClass(kvp.Key, kvp.Value, classInfos?.FirstOrDefault(x => x.TechnicalName == kvp.Key.FullName));
				ConfigurationClass currentClass = Classes?.FirstOrDefault(x => x.type == newClass.type);
				if (currentClass != null)
				{
					newClass.foldout = currentClass.foldout;
				}
			
				newClasses.Add(newClass);
			}

			Classes = newClasses;
		}

		public void OnGUI()
		{
			_richLabelStyle = _richLabelStyle ?? new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };

			if (_modifiedValueStyle == null)
			{
				ColorUtility.TryParseHtmlString(VTEDConstants.orangeColor, out Color orangeColor);
				_modifiedValueStyle = new GUIStyle(EditorStyles.label)
				{
					richText = true,
					wordWrap = true,
					fontStyle = FontStyle.Bold,
					normal =
					{
						textColor = orangeColor
					}
				};
			}
			
			// _foldoutHeaderStyle = _foldoutHeaderStyle ?? new GUIStyle(EditorStyles.foldoutHeader) { richText = true };
			_foldoutHeaderStyle = _foldoutHeaderStyle ?? new GUIStyle(EditorStyles.foldout)
			{
				richText = true,
				fontStyle = FontStyle.Bold
			};

			_linkLabelStyle = _linkLabelStyle ?? new GUIStyle(EditorStyles.label)
			{
				normal =
				{
					textColor = new Color(0.48f, 0.67f, 0.94f, 1f)
				},
				alignment = TextAnchor.MiddleCenter,
				stretchWidth = false
			};
			
			DisplaySelection();
			DisplayConfiguration();
		}

		private void DisplaySelection()
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(VTEDConstants.leftMargin);
				EditorGUILayout.BeginVertical();
				{
					_foldoutSelectionSetup = EditorGUILayout.Foldout(_foldoutSelectionSetup, $"Selection setup     {_configStatus}", true, _foldoutHeaderStyle);

					if (_foldoutSelectionSetup == false)
					{
						EditorGUILayout.EndVertical();
						GUILayout.Space(VTEDConstants.rightMargin);
						EditorGUILayout.EndHorizontal();
						return;
					}

					EditorGUILayout.BeginVertical(GUI.skin.box);
					EditorGUIUtility.labelWidth = 100f;
					EditorGUILayout.LabelField("Version", $"<b>{_versionStatus}</b>", _richLabelStyle);
					EditorGUILayout.LabelField("Segments", $"<b>{_segmentNames}</b>", _richLabelStyle);
					EditorGUILayout.LabelField("ABTests", $"<b>{_abTestNames}</b>", _richLabelStyle);
					EditorGUILayout.LabelField("Cohorts", $"<b>{_cohortNames}</b>", _richLabelStyle);
					EditorGUIUtility.labelWidth = 150f;
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(VTEDConstants.rightMargin);
			}
			EditorGUILayout.EndHorizontal();
		}
		
		private void DisplayConfiguration()
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(VTEDConstants.leftMargin-2);
				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.BeginHorizontal();
					{
						_foldoutDisplayConfiguration = EditorGUILayout.Foldout(_foldoutDisplayConfiguration, "Display configuration", true, _foldoutHeaderStyle);
						
						if (GUILayout.Button("Copy URL", _linkLabelStyle, GUILayout.Width(58f)))
						{
							EditorGUIUtility.systemCopyBuffer = ConfigurationURL;
							VTEDWindow.ShowToast(new GUIContent("Copied to Clipboard"));
						}
						EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
					}
					EditorGUILayout.EndHorizontal();

					_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);

					if (_foldoutDisplayConfiguration == false)
					{
						EditorGUILayout.EndScrollView();
						EditorGUILayout.EndVertical();
						GUILayout.Space(VTEDConstants.rightMargin+2);
						EditorGUILayout.EndHorizontal();
						return;
					}
					
					if (Classes == null)
					{
						EditorGUILayout.LabelField("Loading configuration data, please wait...");
						
						EditorGUILayout.EndScrollView();
						EditorGUILayout.EndVertical();
						GUILayout.Space(VTEDConstants.rightMargin+2);
						EditorGUILayout.EndHorizontal();
						return;
					}

					if (Classes.Count == 0)
					{
						string message = "There are no classes, in the configuration, that applies to your project.\n" +
						                 "Please ensure that the classTechnicalName are correct and reload the app information.";

						if (_liveVersionExist == false)
						{
							message = "You need to publish a version of your app at least once before being able to request the configuration data.";
						}

						EditorGUILayout.HelpBox(message, MessageType.Error);
						EditorGUILayout.Space();
						
						if (_liveVersionExist == false)
						{
							EditorGUILayout.BeginHorizontal();
							{
								GUILayout.FlexibleSpace();
								if (GUILayout.Button("Go to VoodooTune dashboard"))
								{
									Application.OpenURL($"https://voodootune.voodoo-{VoodooTunePersistentData.SavedServer}.io/games/{_appId}/wip/publish");
								}

								GUILayout.FlexibleSpace();
							}
							EditorGUILayout.EndHorizontal();
						}
						
						EditorGUILayout.EndScrollView();
						EditorGUILayout.EndVertical();
						GUILayout.Space(VTEDConstants.rightMargin+2);
						EditorGUILayout.EndHorizontal();
						return;
					}

					ShowConfig();

					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(VTEDConstants.rightMargin+2);
			}
			EditorGUILayout.EndHorizontal();
		}

		public void ExpandAll(bool enable)
		{
			if (Classes == null)
			{
				return;
			}

			_foldoutSelectionSetup = enable;
			_foldoutDisplayConfiguration = enable;
			
			foreach (var configClass in Classes)
			{
				configClass.foldout = enable;
			}
		}

		private void ShowConfig()
		{
			foreach (var configClass in Classes)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				{
					DisplayClassTitle(configClass);
					if (configClass.foldout)
					{
						DisplayClassContent(configClass);
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void DisplayClassTitle(ConfigurationClass configurationClass)
		{
			EditorGUILayout.BeginHorizontal();
			{
				configurationClass.foldout = EditorGUILayout.Foldout(configurationClass.foldout, new GUIContent(configurationClass.type.Name, configurationClass.type.FullName), true);
				if (GUILayout.Button("Show class file", _linkLabelStyle, GUILayout.Width(90f)))
				{
					string[] guids = AssetDatabase.FindAssets(configurationClass.type.Name);
					foreach (string guid in guids)
					{
						MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
				
						if (script.GetClass() != configurationClass.type)
						{
							continue;
						}
							
						EditorGUIUtility.PingObject(script);
						break;
					}
				}
				EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
			}
			EditorGUILayout.EndHorizontal();
		}
		
		private void DisplayClassContent(ConfigurationClass configurationClass)
		{
			EditorGUI.indentLevel++;
			for (var i = 0; i < configurationClass.instances.Count; i++)
			{
				EditorGUILayout.LabelField($"Instance {i}");
				EditorGUI.indentLevel++;
				foreach (var field in configurationClass.fields)
				{
					if (field.instanceIndex != i)
					{
						continue;
					}
					
					GUIStyle currentStyle = field.isDefaultValue ? _richLabelStyle : _modifiedValueStyle;
					EditorGUILayout.LabelField(field.label, field.value, currentStyle);
				}
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
		}
		
		public void ApplyVoodooConfig(IReadOnlyList<ABTest> abTests, IReadOnlyList<Segment> segments)
		{
			if (Classes == null || Classes.Count == 0)
			{
				return;
			}

			ConfigurationClass configurationClass = Classes.FirstOrDefault(x => x.type == typeof(VoodooConfig));
			if (configurationClass == null)
			{
				return;
			}
			
			VoodooConfig config = configurationClass.instances[0] as VoodooConfig;
			_configStatus = string.IsNullOrEmpty(config?.ConfigurationId) == false ? $"<color={VTEDConstants.greenColor}>Default value</color>" : $"<color={VTEDConstants.orangeColor}>Simulation</color>";
			
			List<Segment> currentSegments = segments.Where(s => config.SegmentIds.Contains(s.Id)).ToList();
			List<ABTest> currentABTests = abTests.Where(abTest => config.AbTestIdsToList.Contains(abTest.Id)).ToList();
			List<Cohort> currentCohorts = new List<Cohort>();
			foreach (ABTest currentAbTest in currentABTests)
			{
				Cohort currentCohort = currentAbTest.Cohorts.FirstOrDefault(cohort => config.CohortIdsToList.Contains(cohort.Id));
				currentCohorts.Add(currentCohort);
			}

			_segmentNames = currentSegments.Count > 0 ? string.Join(", ", currentSegments.Select(x => x.Name)): "-";
			_abTestNames = currentABTests.Count > 0 ? string.Join(", ", currentABTests.Select(x => x.Name)): "-";
			_cohortNames = currentCohorts.Count > 0 ? string.Join(", ", currentCohorts.Select(x => x.Name)): "-";
		}
	}
}