using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Voodoo.Sauce.Core
{
    public class Request : IRequest
    {

        private MonoBehaviour _monoBehaviour;

        public void Initialize(MonoBehaviour monoBehaviour)
        {
            _monoBehaviour = monoBehaviour;
        }
        
        public void Get(string url, Dictionary<string, string> headers, Action<UnityWebRequest> onSuccess, Action<UnityWebRequest> onError)
        {
            _monoBehaviour.StartCoroutine(doRequest(url, null, null, headers, onSuccess, onError));
        }

        public void Post(string url, Dictionary<string, string> data, Dictionary<string, string> headers, Action<UnityWebRequest> onSuccess,
                                Action<UnityWebRequest> onError)
        {
            _monoBehaviour.StartCoroutine(doRequest(url, null, data, headers, onSuccess, onError));
        }

        public void PostJson(string url, string jsonData, Dictionary<string, string> headers, Action<UnityWebRequest> onSuccess, 
                                    Action<UnityWebRequest> onError)
        {
            _monoBehaviour.StartCoroutine(doRequest(url, jsonData, null, headers, onSuccess, onError));
        }

        private IEnumerator doRequest(string url, string jsonData, Dictionary<string, string> data, Dictionary<string, string> headers,
                                             Action<UnityWebRequest> onSuccess, Action<UnityWebRequest> onError)
        {
            UnityWebRequest request;

            if (jsonData != null) {
                var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            } else if (data != null) {
                var formData = new WWWForm();
                foreach (var param in data)
                    formData.AddField(param.Key, param.Value);
                request = UnityWebRequest.Post(url, formData);
            } else {
                request = UnityWebRequest.Get(url);
            }
            
            request.timeout = 3;

            if (headers != null) {
                foreach (var param in headers)
                    request.SetRequestHeader(param.Key, param.Value);
            }

            request.chunkedTransfer = false;
            yield return request.SendWebRequest();

            if (string.IsNullOrEmpty(request.error) && request.responseCode >= 200 && request.responseCode < 300)
            {
                onSuccess?.Invoke(request);
            }
            else
            {
                onError?.Invoke(request);
            }
        }
    }
}