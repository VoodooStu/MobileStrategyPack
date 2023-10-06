using UnityEngine;
using UnityEngine.Networking;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    public class KitchenAPIRequest : IKitchenAPIRequest
    {

#region Properties
        
        private readonly UnityWebRequest _request;
        
#endregion

#region Constructors
        
        public KitchenAPIRequest() => _request = UnityWebRequest.Get("");
        
#endregion

#region IKitchenAPIRequest interface implementation

        public string Url {
            get => _request.url;
            set => _request.url = value;
        }

        public int Timeout {
            get => _request.timeout;
            set => _request.timeout = value;
        }

        public bool IsDone {
            get => _request.isDone;
            set { }
        }

        public bool IsHttpError {
            get => _request.isHttpError;
            set { }
        }

        public bool IsNetworkError {
            get => _request.isNetworkError;
            set { }
        }
        
        public string Error {
            get => _request.error;
            set { }
        }
        
        public string TextResponse {
            get => _request.downloadHandler.text;
            set { }
        }

        public bool IsLiveRequest {
            get => true;
            set { }
        }
        
        public string FilesPath {
            get => "";
            set { }
        }

        public YieldInstruction SendWebRequest() => _request.SendWebRequest();

#endregion
    }
}