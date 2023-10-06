using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Voodoo.Tune.Network
{
    public static class WebRequest
    {
        private static int DefaultTimeoutInSeconds = 10;
        private static Dictionary<int, IWebRequestHandler> idToResult = new Dictionary<int, IWebRequestHandler>();

        private static CancellationTokenSource _cancellationTokenSource;

        public static bool IsCancellationRequested => _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested;

        public static void CancelAllTasks()
        {
            _cancellationTokenSource?.Cancel();
        }

        public static void Get(string url, Header header = null, Action<UnityWebRequest> onSuccess = null, Action<UnityWebRequest> onError = null, int timeoutInSeconds = -1)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            SendAndCache(request, header, onSuccess, onError, timeoutInSeconds);
        }
        
        public static async Task<UnityWebRequest> GetAsync(string url, Header header = null, int timeoutInSeconds = -1)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            await SendAndCacheAsync(request, header, timeoutInSeconds);
            return request;
        }

        public static void Put(string url, string content, Header header = null,  Action<UnityWebRequest> onSuccess = null, Action<UnityWebRequest> onError = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(url, content);
            request.uploadHandler.contentType = "application/json";
            
            SendAndCache(request, header, onSuccess, onError);
        }

        public static async Task<UnityWebRequest> PutAsync(string url, string content, Header header = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(url, content);
            request.uploadHandler.contentType = "application/json";

            await SendAndCacheAsync(request, header);
            return request;
        }

        public static void Post(string url, string content, Header header = null, Action<UnityWebRequest> onSuccess = null, Action<UnityWebRequest> onError = null)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(content)))
            {
                uploadHandler = {contentType = "application/json"}
            };

            SendAndCache(request, header, onSuccess, onError);
        }
        
        public static async Task<UnityWebRequest> PostAsync(string url, string content, Header header = null)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(content)))
            {
                uploadHandler = {contentType = "application/json"}
            };

            await SendAndCacheAsync(request, header);
            return request;
        }

        private static void SendAndCache(UnityWebRequest request, Header header, Action<UnityWebRequest> onSuccess = null, Action<UnityWebRequest> onError = null, int timeoutInSeconds =-1) 
        {
            ApplyTimeout(request, timeoutInSeconds);
            ApplyHeader(request, header);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            if (asyncOperation == null)
            {
                return;
            }

            idToResult.Add(asyncOperation.GetHashCode(), new WebRequestHandler(onSuccess, onError));
            asyncOperation.completed += OnAsyncOperationComplete;
        }

        private static async Task SendAndCacheAsync(UnityWebRequest request, Header header, int timeoutInSeconds = -1) 
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            CancellationTokenSource cts = _cancellationTokenSource;

            ApplyTimeout(request, timeoutInSeconds);
            ApplyHeader(request, header);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            
            if (asyncOperation == null)
            {
                return;
            }

            while (asyncOperation.isDone == false && cts.IsCancellationRequested == false)
            {
                await Task.Yield();
            }

            if (cts.IsCancellationRequested)
            {
                Debug.Log("The following web request got canceled : " + request.url);
                cts.Token.ThrowIfCancellationRequested();
            }
        }

        private static void ApplyTimeout(UnityWebRequest request, int timeoutInSeconds = -1)
        {
            request.timeout = timeoutInSeconds >= 0 ? timeoutInSeconds : DefaultTimeoutInSeconds;
        }

        private static void ApplyHeader(UnityWebRequest request, Header header)
        {
            if (header == null || string.IsNullOrEmpty(header.value))
            {
                return;
            }

            for (int i = 0; i < header.value.Length; i++)
            {
                char character = header.value[i];
                if (char.IsLetterOrDigit(character) == false && character != ' ' && character != '-')
                {
                    return;
                }
            }

            request.SetRequestHeader(header.name, header.value);
        }

        private static void OnAsyncOperationComplete(AsyncOperation operation)
        {
            UnityWebRequestAsyncOperation webOperation = operation as UnityWebRequestAsyncOperation;
            if (webOperation == null)
            {
                return;
            }

            IWebRequestHandler handler = idToResult.ContainsKey(webOperation.GetHashCode()) ? idToResult[webOperation.GetHashCode()] : null;
            if (handler == null)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(webOperation.webRequest.error))
            {
                handler.OnSuccess(webOperation.webRequest);
            }
            else
            {
                handler.OnError(webOperation.webRequest);
            }

            webOperation.webRequest.Dispose();
            idToResult.Remove(operation.GetHashCode());
        }
        
        public static bool HadErrors(Task<UnityWebRequest> request) => string.IsNullOrEmpty(request.Result.error) == false;
        public static bool HadErrors(UnityWebRequest request) => string.IsNullOrEmpty(request.error) == false;
    }
}