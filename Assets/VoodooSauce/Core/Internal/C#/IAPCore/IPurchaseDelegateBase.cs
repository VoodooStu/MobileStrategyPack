using UnityEngine.Scripting;

namespace Voodoo.Sauce.Core
{
    
    /// <summary>
    /// This class is just an intermediary class to allow transition to IPurchaseDelegateWithInfo to be done without
    /// causing breaking changes in game's side. Please do not use this interface anywhere outside of VoodooSauce module
    /// and use IPurchaseDelegateWithInfo instead!
    /// </summary>
    [Preserve]
    public interface IPurchaseDelegateBase
    {
        /// <summary>
        ///     Called when the initialization of the IAP system has succeeded.
        /// </summary>
        void OnInitializeSuccess();

        /// <summary>
        ///     Called if for any reason the initialization of the IAP system has failed.
        /// </summary>
        /// <param name="reason"></param>
        void OnInitializeFailure(VoodooInitializationFailureReason reason);
    }
}