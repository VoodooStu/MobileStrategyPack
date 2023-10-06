using UnityEngine.Networking;

namespace Voodoo.Tune.Network
{
    public interface IWebRequestHandler
    {
        void OnSuccess(UnityWebRequest webRequest);

        void OnError(UnityWebRequest webRequest);
    }
}