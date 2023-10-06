using System.Collections.Generic;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Internal.IntegrationCheck;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// The parent of every module class.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// This method will be called to check if everything is well integrated.
        /// </summary>
        /// <param name="settings">Current voodoo settings</param>
        /// <returns>List of messages you want to display to the developer. Errors and Warnings can be displayed.</returns>
        List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings);
    }
}
