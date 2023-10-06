using System.Reflection;
using UnityEngine;

namespace Voodoo.Tune.Debugger.Editor
{
	public class ClassField
	{
		public int instanceIndex;
		public FieldInfo fieldInfo;
		public GUIContent label;
		public GUIContent value;
		public bool isDefaultValue;

		public ClassField(int instanceIndex, FieldInfo fieldInfo, GUIContent label, GUIContent value, bool isDefaultValue)
		{
			this.instanceIndex = instanceIndex;
			this.fieldInfo = fieldInfo;
			this.label = label;
			this.value = value;
			this.isDefaultValue = isDefaultValue;
		}
	}
}