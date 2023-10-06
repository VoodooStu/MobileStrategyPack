#if NEWTONSOFT_JSON
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Voodoo.Tune.Core
{
	public class JsonFloatToIntConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JValue jsonValue = serializer.Deserialize<JValue>(reader);

			if (jsonValue.Type == JTokenType.Float)
			{
				return Mathf.RoundToInt(jsonValue.Value<float>());
			}

			if (jsonValue.Type == JTokenType.Integer)
			{
				return jsonValue.Value<int>();
			}

			throw new FormatException();
		}

		public override bool CanWrite => false;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}
#endif