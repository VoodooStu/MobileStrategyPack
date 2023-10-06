using System.Collections.Generic;

namespace Voodoo.Sauce.Internal.Common.Utils
{
    /// <summary>
    /// Class used to create a formatted string when using GET with query parameters
    /// Each key/value pair will be converted into a properly formatted string
    /// Example when adding key1: value1, key2: value2 and key3: value3
    /// ToString() Result will be: "?key1=value1&key2=value2&key3=value3"
    /// </summary>
    public class QueryParameters
    {
        private string _baseUrl;
        private readonly Dictionary<string, object> _parameters;
        
        /// <summary>
        /// Constructor of the query
        /// </summary>
        /// <param name="baseUrl">Base URL of the query</param>
        public QueryParameters(string baseUrl)
        {
            _baseUrl = baseUrl;
            _parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add a new parameter to the parameters list
        /// </summary>
        /// <param name="key">Key of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public void Add(string key, object value)
        {
            if (!_parameters.ContainsKey(key)) {
                _parameters.Add(key, value);
            } else {
                _parameters[key] = value;
            }
        }

        /// <summary>
        /// Format the provided parameters with the URL GET query parameter format
        /// </summary>
        /// <returns>?key1=value&key2=value2&key3=value3 ...</returns>
        public string GetFormattedUrl()
        {
            var paramList = new List<string>();
            
            foreach (KeyValuePair<string, object> pair in _parameters) {
                paramList.Add($"{pair.Key}={pair.Value}");
            }

            string str = string.Join("&", paramList);
            return $"{_baseUrl}?{str}";
        }
    }
}