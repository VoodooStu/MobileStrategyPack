using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Voodoo.Tune.Core;

namespace Voodoo.Tune.Debugger.Editor
{
	public class ConfigurationClass
	{
		public Type type;
		public List<object> instances;
		public bool foldout;
		public List<ClassField> fields;

		public ConfigurationClass(Type type, List<object> instances, ClassInfo defaultClass = null, bool foldout = true)
		{
			this.type = type;
			this.instances = instances;
			this.foldout = foldout;
			
			PopulateFields(defaultClass);
		}
		
		private void PopulateFields(ClassInfo defaultClass)
		{
			fields = new List<ClassField>();
			FieldInfo[] classFields = type.GetFields();
			for (var i = 0; i < instances.Count; i++)
			{
				object classInstance = instances[i];
				
				//The number of instances can't be changed once the class is created 
				Dictionary<string,object> defaultClassInstance = defaultClass?.Instances[i].Values;
				
				foreach (FieldInfo fieldInfo in classFields)
				{
					object fieldValue = fieldInfo.GetValue(classInstance);
					IList fieldValueAsList = fieldValue as IList;
					
					GUIContent label = new GUIContent(fieldInfo.Name);
					GUIContent value = fieldValueAsList != null ? new GUIContent($"[{GetListAsString(fieldValueAsList)}]") : new GUIContent(fieldValue + "");
					bool isDefaultValue = true;
					
					if (type != typeof(VoodooConfig) && defaultClassInstance != null && defaultClassInstance.ContainsKey(fieldInfo.Name))
					{
						object defaultValue = defaultClassInstance[fieldInfo.Name];
						IList defaultValueAsList = defaultValue as IList;

						isDefaultValue = fieldValueAsList != null ? IsDefaultValue(fieldValueAsList, defaultValueAsList) : IsDefaultValue(fieldValue, defaultValue);

						if (isDefaultValue == false)
						{
							UpdateLabelTooltip(label, defaultValue, defaultValueAsList);
						}
					}

					ClassField field = new ClassField(i, fieldInfo, label, value, isDefaultValue);
					fields.Add(field);
				}
			}
		}

		private bool IsDefaultValue(IList currentValue, IList defaultValue)
		{
			if (defaultValue == null)
			{
				return true;
			}

			if (currentValue.Count != defaultValue.Count)
			{
				return false;
			}

			for (int j = 0; j < currentValue.Count; j++)
			{
				object a = currentValue[j];
				object b = defaultValue[j];

				if (a.ToString() != b.ToString())
				{
					return false;
				}
			}

			return true;
		}
		
		private bool IsDefaultValue(object currentValue, object defaultValue)
		{
			if (currentValue == null || defaultValue == null)
			{
				return false;
			}
			
			if (currentValue is IConvertible testValueConvertible && defaultValue is IConvertible)
			{
				currentValue = testValueConvertible.ToType(defaultValue.GetType(), System.Globalization.CultureInfo.InvariantCulture);
			}

			return currentValue.Equals(defaultValue);
		}

		private void UpdateLabelTooltip(GUIContent label, object defaultValue, IList defaultValueAsList)
		{
			if (defaultValueAsList != null)
			{
				label.tooltip = "Default Value : " + GetListAsString(defaultValueAsList);
			}
			else if (string.IsNullOrEmpty(defaultValue.ToString()))
			{
				label.tooltip = "Default Value : \"\"";
			}
			else
			{
				label.tooltip = "Default Value : " + defaultValue;
			}
		}

		private string GetListAsString(IList list)
		{
			string res = "";
			foreach (object obj in list)
			{
				res += obj + ", ";
			}

			if (res.Length > 1)
			{
				res = res.Remove(res.Length - 2);
			}

			return res;
		}
	}
}