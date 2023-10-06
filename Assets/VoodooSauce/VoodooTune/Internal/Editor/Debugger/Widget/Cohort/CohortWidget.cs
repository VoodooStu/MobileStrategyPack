using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class CohortWidget : IDataWidget
	{
		private List<ABTest> _values;
		private string[] _displayedOptions;

		private CohortWidgetData _currentData;
		public bool CanApply { get; set; } = true;

		public bool IsSimulating => VoodooTunePersistentData.SavedABTests?.Count > 0 && VoodooTunePersistentData.SavedCohorts?.Count > 0;
		
		public CohortWidget(IReadOnlyList<ABTest> abTests, Platform platform)
		{
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

		//Reload dropdown ABTest
		private void RecoverDropDown()
		{
			if (_values.Count > 0)
			{
				int abTestIndex = 0;
				int cohortIndex = 0;
				
				if (IsSimulating)
				{
					abTestIndex = Mathf.Max(0, _values.FindIndex(x => x.Id == VoodooTunePersistentData.SavedABTests[0].Id));
					cohortIndex = Mathf.Max(0, _values[abTestIndex].Cohorts.FindIndex(x => x.Id == VoodooTunePersistentData.SavedCohorts[0].Id));
				}
				
				ABTest abTest = _values[abTestIndex];
				Cohort cohort = abTest.Cohorts[cohortIndex];
				
				_currentData = new CohortWidgetData(abTest, cohort, abTestIndex, cohortIndex);
				_displayedOptions = _values.Select(x => x.Name.Replace("/","_")).ToArray();
			}
			else
			{
				_currentData = new CohortWidgetData(null, null, 0, 0);
				_displayedOptions = new[] { "-" };
			}
		}

		public void OnGUI()
		{
			if (_values.Count == 0)
			{
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox("There is no ABTest for the current version of your app.", MessageType.Error);
				GUI.backgroundColor = Color.white;
				
				CanApply = false;
			}

			if (CanApply == false)
			{
				GUI.enabled = false;
			}
			
			DisplayCohorts();
			
			GUI.enabled = true;
		}

		private void DisplayCohorts()
		{
			EditorGUIUtility.labelWidth = 100f;
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			_currentData.abTestIndex = EditorGUILayout.Popup("ABTest", _currentData.abTestIndex, _displayedOptions);
			if (EditorGUI.EndChangeCheck())
			{
				_currentData.UpdateABTest(_values);
			}
			
			GUILayout.Space(10f);
			bool isGUIEnabled = GUI.enabled;
			GUI.enabled = _values.Count > 0;
			CanApply = EditorGUILayout.Toggle(CanApply, GUILayout.Width(16f));
			GUI.enabled = isGUIEnabled;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			_currentData.cohortIndex = EditorGUILayout.Popup("Cohort", _currentData.cohortIndex, _currentData.CohortDisplayOptions);
			if (EditorGUI.EndChangeCheck())
			{
				_currentData.UpdateCohort();
			}
			
			GUILayout.Space(29f);
			EditorGUILayout.EndHorizontal();
			
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

			List<ABTest> abTests = new List<ABTest> { _currentData.abTest };
			List<Cohort> cohorts = new List<Cohort> { _currentData.cohort };
			VoodooTunePersistentData.Save(abTests, cohorts);
			
			return true;
		}
	}
}