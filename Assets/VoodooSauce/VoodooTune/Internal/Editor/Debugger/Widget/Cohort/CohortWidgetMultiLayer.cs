using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class CohortWidgetMultiLayer : IDataWidget
	{
		private List<ABTest> _values;
		private string[] _displayedOptions;
		private List<Layer> _layers;
		public bool CanApply { get; set; } = true;
		
		private List<CohortWidgetData> _currentData;

		public bool IsSimulating => VoodooTunePersistentData.SavedABTests?.Count > 0 && VoodooTunePersistentData.SavedCohorts?.Count > 0;

		private GUIStyle _boxStyle;
		
		public CohortWidgetMultiLayer(IReadOnlyList<ABTest> abTests, IReadOnlyList<Layer> layers, Platform platform)
		{
			_layers = new List<Layer>(layers);
			_values = new List<ABTest>();
			foreach (ABTest abtest in abTests)
			{
				bool isRightPlatform = abtest.Platform.AllPlatforms ||
				                       platform == Platform.android ? abtest.Platform.Android != null : abtest.Platform.IOS != null;
				
				if (isRightPlatform)
				{
					_values.Add(abtest);
				}
			}

			_values = _values.OrderBy(x => x.Name).ToList();

			RecoverDropDown();
		}

		private void RecoverDropDown()
		{
			_currentData = new List<CohortWidgetData>();
			if (_values.Count > 0)
			{
				for (var i = 0; i < VoodooTunePersistentData.SavedABTests.Count; i++)
				{
					int abTestIndex = _values.FindIndex(x => x.Id == VoodooTunePersistentData.SavedABTests[i].Id);
					if (abTestIndex == -1)
					{
						continue;
					}

					ABTest abTest = _values[abTestIndex];

					int cohortIndex = Mathf.Max(0, abTest.Cohorts.FindIndex(x => x.Id == VoodooTunePersistentData.SavedCohorts[i].Id));
					Cohort existingCohort = abTest.Cohorts[cohortIndex];

					_currentData.Add(new CohortWidgetData(abTest, existingCohort, abTestIndex, cohortIndex));
				}
				
				if (_currentData.Count == 0)
				{
					ABTest abTest = _values[0];
					Cohort cohort = abTest.Cohorts[0];
					_currentData.Add(new CohortWidgetData(abTest, cohort, 0, 0));
				}
				
				List<string> abTestDisplayNames = new List<string>();
				foreach (ABTest abTest in _values)
				{
					Layer layer = _layers.Find(x => x.Id == abTest.LayerId);
					abTestDisplayNames.Add(abTest.Name.Replace("/", "_") + " | " + layer.Name);
				}

				_displayedOptions = abTestDisplayNames.ToArray();
			}
			else
			{
				_displayedOptions = new[] { "-" };
			}
		}

		public void OnGUI()
		{
			_boxStyle = _boxStyle ?? new GUIStyle(GUI.skin.box)
			{
				margin = new RectOffset(0,0,0,0),
			};
			
			if (_values.Count == 0)
			{
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox("There is no ABTest for the current version of your app.", MessageType.Error);
				GUI.backgroundColor = Color.white;
				
				GUI.enabled = false;
			}
			
			CanApply = _values.Count > 0;

			DisplayCohorts();
			
			GUI.enabled = true;
		}

		private void DisplayCohorts()
		{
			GUILayout.Space(8f);
			bool displayDuplicateLayerMessage = false;
			EditorGUIUtility.labelWidth = 100f;
			if (_values.Count == 0)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Popup("ABTest | Layer", 0, _displayedOptions);
				GUILayout.Space(29f);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Popup("Cohort", 0, _displayedOptions);
				GUILayout.Space(29f);
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(8f);
			}
			else
			{
				int toRemoveIndex = -1;
				for (var i = 0; i < _currentData.Count; i++)
				{
					List<CohortWidgetData> tempCohortWidgetData = new List<CohortWidgetData>(_currentData);
					tempCohortWidgetData.RemoveAt(i);
					bool hasDuplicate = tempCohortWidgetData.Exists(x => x.abTest.LayerId == _currentData[i].abTest.LayerId);
					
					if (hasDuplicate)
					{
						displayDuplicateLayerMessage = true;
						GUI.backgroundColor = Color.red;
					}
					
					EditorGUILayout.BeginVertical(_boxStyle);
					{
						GUI.backgroundColor = Color.white;
						CohortWidgetData data = _currentData[i];
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUI.BeginChangeCheck();
							data.abTestIndex = EditorGUILayout.Popup("ABTest | Layer", data.abTestIndex, _displayedOptions);
							if (EditorGUI.EndChangeCheck())
							{
								data.UpdateABTest(_values);
							}

							GUILayout.Space(10f);
							if (GUILayout.Button("\u2212", GUILayout.Width(16f)))
							{
								toRemoveIndex = i;
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUI.BeginChangeCheck();
							data.cohortIndex = EditorGUILayout.Popup("Cohort", data.cohortIndex, data.CohortDisplayOptions);
							if (EditorGUI.EndChangeCheck())
							{
								data.UpdateCohort();
							}
							GUILayout.Space(29f);
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
					GUILayout.Space(8f);
				}
			
				if (toRemoveIndex != -1)
				{
					_currentData.RemoveAt(toRemoveIndex);
				}
			}
			
			CanApply = _currentData.Count > 0 && displayDuplicateLayerMessage == false;

			GUILayout.Space(4f);
			
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(EditorGUIUtility.labelWidth + 4);
				if (GUILayout.Button("\uFF0B Add a new A/B test and cohort"))
				{
					CohortWidgetData cohortWidgetData = _currentData.Count == 0 ? new CohortWidgetData(_values[0], _values[0].Cohorts[0], 0, 0) : new CohortWidgetData(_currentData.Last());
					_currentData.Add(cohortWidgetData);
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(29f);
			}
			EditorGUILayout.EndHorizontal();
			
			if (displayDuplicateLayerMessage)
			{
				EditorGUILayout.Space();
				
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox(VTEDConstants.duplicateLayerMessage, MessageType.Error);
				GUI.backgroundColor = Color.white;
			}

			EditorGUIUtility.labelWidth = 150f;
		}
		
		/// <summary>
		/// Returns true if the save did happen. Return false otherwise 
		/// </summary>
		public bool Save()
		{
			if (CanApply == false)
			{
				return false;
			}

			List<ABTest> abTests = _currentData.Select(x => x.abTest).ToList();
			List<Cohort> cohorts  = _currentData.Select(x => x.cohort).ToList();

			VoodooTunePersistentData.Save(abTests, cohorts);
			return true;
		}
	}
}