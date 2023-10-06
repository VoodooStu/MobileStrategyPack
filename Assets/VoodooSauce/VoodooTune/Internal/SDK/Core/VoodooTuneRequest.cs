using System.Threading.Tasks;
#if NEWTONSOFT_JSON
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Tune.Network;

namespace Voodoo.Tune.Core
{
	public static class VoodooTuneRequest
	{
#if NEWTONSOFT_JSON
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(),
                new JsonFloatToIntConverter()
            },
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
#endif
        
        public static async Task<T> GetAsync<T>(string url, Header header = null) where T : class
        {
            var request = await WebRequest.GetAsync(url, header);
			
            if (RequestFailed(request))
            {
                return null;
            }
            
            if (typeof(T) == typeof(string))
            {
                return request.downloadHandler.text as T;
            }
            
#if NEWTONSOFT_JSON
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
#else
            return await Task.FromResult<T>(default);
#endif
        }

        public static async Task<T> PostAsync<T>(string url, string content, Header header = null) where T : class
        {
            var request = await WebRequest.PostAsync(url, content, header);
			
            if (RequestFailed(request))
            {
                return null;
            }
            
            if (typeof(T) == typeof(string))
            {
                return request.downloadHandler.text as T;
            }
			
#if NEWTONSOFT_JSON
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
#else
            return await Task.FromResult<T>(default);
#endif
        }

        public static async Task<T> PutAsync<T>(string url, string content, Header header = null) where T : class
        {
            var request = await WebRequest.PutAsync(url, content, header);
			
            if (RequestFailed(request))
            {
                return null;
            }
            
            if (typeof(T) == typeof(string))
            {
                return request.downloadHandler.text as T;
            }
			
#if NEWTONSOFT_JSON
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
#else
            return await Task.FromResult<T>(default);
#endif
        }
        
        private static bool RequestFailed(UnityWebRequest webRequest)
        {
            if (string.IsNullOrEmpty(webRequest.error) == false)
            {
                Debug.LogError(webRequest.error + " : " + webRequest.downloadHandler.text);
                return true;
            }
            
            try //No error fired but the url wasn't set up properly
            {
#if NEWTONSOFT_JSON
                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text);
                bool? success = jsonObject.GetValue("success")?.Value<bool>();

                if (success == null || success.Value)
                {
                    return false;
                }
                
                Debug.LogError("error : " + webRequest.url + ", " + jsonObject.GetValue("reason") + ". " + jsonObject.GetValue("details"));
#endif
                
                return true;
            }
            catch //Success
            {
                return false;
            }
        }

	}
}