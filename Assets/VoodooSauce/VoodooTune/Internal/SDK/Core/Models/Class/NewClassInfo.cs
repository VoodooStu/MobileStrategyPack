using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Voodoo.Tune.Utils;

namespace Voodoo.Tune.Core
{
	public class NewClassInfo
	{
		public string displayName;
		public string technicalName;
		public string description;
		public string bundleId;
		public List<Attribute> attributes;
		public List<Instance> instances;
		public bool multiInstance;
		public string lockedByLayer;
		
		#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonIgnore]
#endif
		public Type BaseType { get; }

		public NewClassInfo(Type type)
		{
			BaseType = type;
			displayName = Regex.Replace(type.Name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
			technicalName = type.FullName;
			bundleId = Application.identifier;
			multiInstance = false;
			lockedByLayer = "";
			
			FieldInfo[] fields = type.GetFields();
			object typeInstance = Activator.CreateInstance(type);

			attributes = new List<Attribute>();
			instances = new List<Instance> { new Instance() };

			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fieldInfo = fields[i];

				var attribute = new Attribute(fieldInfo);
				if (attribute.TypeSupported == false)
				{
					continue;
				}
				
				attributes.Add(attribute);
				instances[0].values.Add(fieldInfo.Name, fieldInfo.GetValue(typeInstance));
				
			}

			if (type.GetConstructor(Type.EmptyTypes) == null)
			{
				Debug.Log($"The type {type} doesn't have a default constructor. The default values could not be set automatically"); ;
			}
		}

		public NewClassInfo(ClassInfo classInfo)
		{
			BaseType = TypeUtility.GetType(classInfo.TechnicalName);
			displayName = classInfo.DisplayName;
			technicalName = classInfo.TechnicalName;
			description = classInfo.Description;
			bundleId = classInfo.BundleId;
			multiInstance = classInfo.MultiInstance;
			lockedByLayer = classInfo.LockedByLayer;
			
			attributes = new List<Attribute>();
			foreach (ClassInfo.Attribute attribute in classInfo.Attributes)
			{
				attributes.Add(new Attribute(attribute));
			}
			
			instances = new List<Instance>();
			foreach (ClassInfo.Instance instance in classInfo.Instances)
			{
				instances.Add(new Instance(instance));
			}
		}

		public class Instance
		{
			public string id;
			public Dictionary<string, object> values;
			public bool enabled;

			public Instance()
			{
				id = Guid.NewGuid().ToString();
				values = new Dictionary<string, object>();
				enabled = true;
			}

			public Instance(ClassInfo.Instance instance)
			{
				id = instance.Id;
				enabled = instance.Enabled;
				
				values = new Dictionary<string, object>();
				foreach (KeyValuePair<string,object> kvp in instance.Values)
				{
					values.Add(kvp.Key, kvp.Value);
				}
			}
		}

		public class Attribute
		{
			private static readonly List<string> SupportedTypes = new List<string> { "string", "float", "integer", "boolean", "string[]", "float[]", "integer[]" };

			public string displayName;
			public string technicalName;
			public string type;
			public List<string> availableValues;

			#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonIgnore]
#endif
			public bool TypeSupported => SupportedTypes.IndexOf(type) >= 0;

			//TODO : define what we want to do here (keep local values ? keep online values ? Mix them ?
			public Attribute(ClassInfo.Attribute attribute)
			{
				displayName = attribute.DisplayName;
				technicalName = attribute.TechnicalName;
				type = attribute.Type;
				if (attribute.AvailableValues != null)
				{
					availableValues = new List<string>(attribute.AvailableValues);
				}
			}
			
			public Attribute(FieldInfo fieldInfo)
			{
				var fieldType = fieldInfo.FieldType;

				displayName = Regex.Replace(fieldInfo.Name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
				technicalName = fieldInfo.Name;
				availableValues = fieldType.IsEnum ? Enum.GetNames(fieldType).ToList() : null;
				type = ToJSNaming(fieldType);
			}

			string ToJSNaming(Type type)
			{
				if (type.IsEnum)
				{
					return "string";
				}

				switch (type.FriendlyName())
				{
					case "String":
						return "string";
					case "Int32":
						return "integer";
					case "Single":
						return "float";
					case "Double":
						return "float";
					case "Boolean":
						return "boolean";
					case "String[]":
						return "string[]";
					case "Int32[]":
						return "integer[]";
					case "Single[]":
						return "float[]";
					case "List[String]":
						return "string[]";
					case "List[Int32]":
						return "integer[]";
					case "List[Single]":
						return "float[]";
					default:
						return type.FriendlyName();
				}
			}
		}
	}
}