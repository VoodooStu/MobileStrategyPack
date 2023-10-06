using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class SegmentWidget : IDataWidget
	{
		private int _index;
		private List<Segment> _values;
		private string[] _displayedOptions;

		public bool CanApply { get; set; } = true;
		public bool IsSimulating => VoodooTunePersistentData.SavedSegments?.Count > 0;
		
		public SegmentWidget(IReadOnlyList<Segment> segments)
		{
			_index = 0;
			_values = new List<Segment>(segments);
			
			RecoverSegments();
		}

		private void RecoverSegments()
		{
			//Reload dropdown Segments
			if (_values.Count > 0)
			{
				if (VoodooTunePersistentData.SavedSegments.Count > 0)
				{
					foreach (Segment savedSegment in VoodooTunePersistentData.SavedSegments)
					{
						int savedSegmentIndex = _values.FindIndex(x => x.Id == savedSegment.Id);
						if (savedSegmentIndex == -1)
						{
							continue;
						}
						
						_index += 1 << savedSegmentIndex;
					}
				}
				
				_displayedOptions = _values.Select(x => x.Name.Replace("/","_")).ToArray();
			}
			else
			{
				_displayedOptions = new[] { "-" };
			}
		}
		
		public List<Segment> GetSelection()
		{
			List<Segment> res = new List<Segment>();
			for (int i = 0; i < _values.Count; i++)
			{
				if ((_index & 1 << i) != 0)
				{
					res.Add(_values[i]);
				}
			}

			return res;
		}

		public void OnGUI()
		{
			if (_values.Count == 0)
			{
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox("There are no Segments for the current version of your app.", MessageType.Error);
				GUI.backgroundColor = Color.white;
				
				CanApply = false;
			}
			
			if (CanApply == false)
			{
				GUI.enabled = false;
			}
			
			DisplaySegments();

			GUI.enabled = true;
		}
		
		private void DisplaySegments()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth = 100f;
			_index = EditorGUILayout.MaskField("Segments", _index, _displayedOptions);
			EditorGUIUtility.labelWidth = 150f;
			GUILayout.Space(10f);
			
			bool isGUIEnabled = GUI.enabled;
			GUI.enabled = _values.Count > 0;;
			CanApply = EditorGUILayout.Toggle(CanApply, GUILayout.Width(16f));
			GUI.enabled = isGUIEnabled;
			EditorGUILayout.EndHorizontal();
		}
		
		/// <summary>
		/// Returns true if the save did happen. Return false otherwise 
		/// </summary>
		public bool Save()
		{
			if (CanApply == false || _index == 0)
			{
				return false;
			}

			List<Segment> segments = new List<Segment>();
			for (int i = 0; i < _values.Count; i++)
			{
				if ((_index & 1 << i) != 0)
				{
					segments.Add(_values[i]);
				}
			}

			VoodooTunePersistentData.Save(segments);
			
			return true;
		}
	}
}