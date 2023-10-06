using System;
using UnityEngine.Networking;

namespace Voodoo.Tune.Network
{
	public abstract class AbstractWebRequestHandler : IWebRequestHandler
	{
		protected Action<UnityWebRequest> _onSuccess;
		protected Action<UnityWebRequest> _onError;

		protected AbstractWebRequestHandler(Action<UnityWebRequest> onSuccess, Action<UnityWebRequest> onError)
		{
			_onSuccess = onSuccess;
			_onError = onError;
		}

		public virtual void OnSuccess(UnityWebRequest webRequest)
		{
			_onSuccess?.Invoke(webRequest);
		}

		public virtual void OnError(UnityWebRequest webRequest)
		{
			//If you want to use this, add a reference to the VoodooStoreHelpers assembly definition (asmdef) in the WebRequestUtility asmdef
#if DEBUG_WEB_REQUEST
			DebugHelper.DumpWebResponse(GetType().Name + "Errors.json", webRequest.downloadHandler.text);
#endif
			UnityEngine.Debug.LogError("unity web request with url : " + webRequest.url + " " + webRequest.responseCode.ToString() + "\nFailed with message : \n" + webRequest.error);

			_onError?.Invoke(webRequest);
		}
	}
}