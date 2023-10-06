using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class WidgetManager
	{
		public ConfigurationWidget configurationWidget;
		
		private readonly IReadOnlyList<ABTest> _abTests;
		private readonly IReadOnlyList<Segment> _segments;
		private readonly IReadOnlyList<Layer> _layers;
		private readonly IReadOnlyList<ClassInfo> _classInfos;

		private readonly SegmentWidget _segmentWidget;
		private readonly IDataWidget _cohortWidget;

		private GUIStyle _infoMiniLabelStyle;
		private bool _applyAll;

		public bool CanApply => _cohortWidget.CanApply || (_segmentWidget.CanApply && _segmentWidget.GetSelection().Count > 0);
		public bool IsSimulating => _cohortWidget.IsSimulating || _segmentWidget.IsSimulating;

		public WidgetManager(IReadOnlyList<ABTest> abTests, IReadOnlyList<Layer> layers,
			IReadOnlyList<Segment> segments, IReadOnlyList<ClassInfo> classInfos, Platform platform)
		{
			_abTests = abTests;
			_segments = segments;
			_layers = layers;
			_classInfos = classInfos;

			_segmentWidget = new SegmentWidget(segments);
			
			if (_layers.Count > 1)
			{
				_cohortWidget = new CohortWidgetMultiLayer(abTests, layers, platform);
			}
			else
			{
				_cohortWidget = new CohortWidget(abTests, platform);
			}
		}
		
		public async Task UpdateConfigurationEditor(Server server, string appId, string bundleId, string platform, Version version)
		{
			configurationWidget = new ConfigurationWidget(server, appId, bundleId, platform, Application.version, version);

			VTEDWindow.RepaintWindow();
			
			try
			{
				await configurationWidget.Simulate(_classInfos);
			}
			catch
			{
				return;
			}

			configurationWidget.ApplyVoodooConfig(_abTests, _segments);
		}

		public void ShouldSaveAll(bool applyAll)
		{
			if (_segmentWidget != null)
			{
				_segmentWidget.CanApply = applyAll;
			}

			if (_cohortWidget != null)
			{
				_cohortWidget.CanApply = applyAll;
			}
		}

		public void OnGUI()
		{
			_infoMiniLabelStyle = _infoMiniLabelStyle ?? new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleRight
			};
			
			_segmentWidget?.OnGUI();
			_cohortWidget?.OnGUI();
						
			EditorGUILayout.Space();
						
			if (_layers.Count == 1)
			{
				DisplayApplyAll();
			}
		}

		private void DisplayApplyAll()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("\uFFFD Select one of the criteria or all of them before applying", _infoMiniLabelStyle);
				GUILayout.Space(10f);

				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.labelWidth = 30;
				_applyAll = EditorGUILayout.Toggle("All", _applyAll, GUILayout.Width(48));
				EditorGUIUtility.labelWidth = 150;
				if (EditorGUI.EndChangeCheck())
				{
					ShouldSaveAll(_applyAll);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		public string Save()
		{
			bool saveCohort = _cohortWidget.Save();
			bool saveSegments = _segmentWidget.Save();

			if (saveCohort == false && saveSegments == false)
			{
				return string.Empty;
			}

			string message = "Assign successful\n";
			if (saveCohort)
			{
				message += $"\nABTest: [{string.Join(", ", VoodooTunePersistentData.SavedABTests.Select(x => x.Name))}]";
				message += $"\nCohort: [{string.Join(", ", VoodooTunePersistentData.SavedCohorts.Select(x => x.Name))}]";
			}
			
			if (saveSegments)
			{
				message += $"\nSegments: [{string.Join(", ", VoodooTunePersistentData.SavedSegments.Select(x => x.Name))}]";
			}

			return message;
		}
	}
}