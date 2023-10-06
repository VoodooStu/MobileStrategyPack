using System;
using UnityEngine.Networking;

namespace Voodoo.Tune.Network
{
	public class WebRequestHandler : AbstractWebRequestHandler
	{
		public WebRequestHandler(Action<UnityWebRequest> onSuccess, Action<UnityWebRequest> onError) : base(onSuccess, onError) { }
	}
}