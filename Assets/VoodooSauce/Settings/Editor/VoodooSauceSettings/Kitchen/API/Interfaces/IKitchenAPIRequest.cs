using UnityEngine;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This interface is used to declare:
     * - the real web request object used in prod/development environments
     * - the mocked web request object used for the integration tests.
     *
     * The properties and the methods are inspired by the class 'UnityWebRequest'.
     */
    public interface IKitchenAPIRequest
    {
        string Url { get; set; }
        int Timeout { get; set; }
        bool IsDone { get; set; }
        bool IsHttpError { get; set; }
        bool IsNetworkError { get; set; }
        string Error { get; set; }
        string TextResponse { get; set; }
        bool IsLiveRequest { get; set; }
        string FilesPath { get; set; }

        YieldInstruction SendWebRequest();
    }
}