
using System.Collections.Generic;
using System.Globalization;

public static class MaxNativeAdsSdkUtils
{
    /// <summary>
    /// Tries to get a dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the dictionary</param>
    /// <param name="key">The key to be used to retrieve the dictionary</param>
    /// <param name="defaultValue">The default value to be returned when a value for the given key is not found.</param>
    /// <returns>The dictionary for the given key if available, the default value otherwise.</returns>
    public static Dictionary<string, object> GetDictionaryFromDictionary(IDictionary<string, object> dictionary,
                                                                         string key,
                                                                         Dictionary<string, object> defaultValue = null)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object value) && value is Dictionary<string, object> objects) {
            return objects;
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a list from the dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the list</param>
    /// <param name="key">The key to be used to retrieve the list</param>
    /// <param name="defaultValue">The default value to be returned when a value for the given key is not found.</param>
    /// <returns>The list for the given key if available, the default value otherwise.</returns>
    public static List<object> GetListFromDictionary(IDictionary<string, object> dictionary, string key, List<object> defaultValue = null)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object value) && value is List<object> list)
        {
            return list;
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a <c>string</c> value from dictionary for the given key if available, returns the default value if unavailable.  
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the <c>string</c> value.</param>
    /// <param name="key">The key to be used to retrieve the <c>string</c> value.</param>
    /// <param name="defaultValue">The default value to be returned when a value for the given key is not found.</param>
    /// <returns>The <c>string</c> value from the dictionary if available, the default value otherwise.</returns>
    public static string GetStringFromDictionary(IDictionary<string, object> dictionary, string key, string defaultValue = "")
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object value) && value != null)
        {
            return value.ToString();
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a <c>int</c> value from dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the <c>int</c> value.</param>
    /// <param name="key">The key to be used to retrieve the <c>int</c> value.</param>
    /// <param name="defaultValue">The default value to be returned when a <c>int</c> value for the given key is not found.</param>
    /// <returns>The <c>int</c> value from the dictionary if available, the default value otherwise.</returns>
    public static int GetIntFromDictionary(IDictionary<string, object> dictionary, string key, int defaultValue = 0)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object obj) &&
            obj != null &&
            int.TryParse(InvariantCultureToString(obj), NumberStyles.Any, CultureInfo.InvariantCulture, out int value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a <c>long</c> value from dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the <c>long</c> value.</param>
    /// <param name="key">The key to be used to retrieve the <c>long</c> value.</param>
    /// <param name="defaultValue">The default value to be returned when a <c>long</c> value for the given key is not found.</param>
    /// <returns>The <c>long</c> value from the dictionary if available, the default value otherwise.</returns>
    public static long GetLongFromDictionary(IDictionary<string, object> dictionary, string key, long defaultValue = 0L)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object obj) &&
            obj != null &&
            long.TryParse(InvariantCultureToString(obj), NumberStyles.Any, CultureInfo.InvariantCulture, out long value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a <c>float</c> value from dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the <c>float</c> value.</param>
    /// <param name="key">The key to be used to retrieve the <c>float</c> value.</param>
    /// <param name="defaultValue">The default value to be returned when a <c>string</c> value for the given key is not found.</param>
    /// <returns>The <c>float</c> value from the dictionary if available, the default value otherwise.</returns>
    public static float GetFloatFromDictionary(IDictionary<string, object> dictionary, string key, float defaultValue = 0F)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object obj) &&
            obj != null &&
            float.TryParse(InvariantCultureToString(obj), NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Tries to get a <c>double</c> value from dictionary for the given key if available, returns the default value if unavailable.
    /// </summary>
    /// <param name="dictionary">The dictionary from which to get the <c>double</c> value.</param>
    /// <param name="key">The key to be used to retrieve the <c>double</c> value.</param>
    /// <param name="defaultValue">The default value to be returned when a <c>double</c> value for the given key is not found.</param>
    /// <returns>The <c>double</c> value from the dictionary if available, the default value otherwise.</returns>
    public static double GetDoubleFromDictionary(IDictionary<string, object> dictionary, string key, int defaultValue = 0)
    {
        if (dictionary == null) return defaultValue;

        if (dictionary.TryGetValue(key, out object obj) &&
            obj != null &&
            double.TryParse(InvariantCultureToString(obj), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts the given object to a string without locale specific conversions.
    /// </summary>
    private static string InvariantCultureToString(object obj) => string.Format(CultureInfo.InvariantCulture, "{0}", obj);
    
}
