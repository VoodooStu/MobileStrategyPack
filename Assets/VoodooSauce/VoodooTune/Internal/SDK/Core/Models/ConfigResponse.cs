using UnityEngine.Networking;

namespace Voodoo.Tune.Core
{
	public class ConfigResponse
	{
		public string Url { get; protected set; }
		public string Response { get; protected set; }
		public string Error { get; protected set; }
		public long ResponseCode { get; protected set; }
		public double DurationInMilliseconds { get; protected set; }
		public UnityWebRequest Request { get; private set; }
		
		public ConfigResponse(string url, UnityWebRequest request, double durationInMilliseconds)
		{
			Url = url;
			Request = request;
			DurationInMilliseconds = durationInMilliseconds;
			
			Response = request.downloadHandler?.text;
			Error = request.error;
			ResponseCode = request.responseCode;
		}
	}
}