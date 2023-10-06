using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.IntegrationCheck;
using Voodoo.Sauce.Privacy;

[assembly: InternalsVisibleTo(("Assembly-CSharp-Editor"))]
// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    
    /// <summary>
    /// This static class manages every module of the VoodooSauce.
    ///
    /// A little reminder about module classes: all of them inherit <see cref="IModule"/> and there are two classes per module:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The core class is the basic module class with default behavior.
    /// It defines every method needed to be called from outside of the module itself and it's always located in the VoodooSauceCore.
    /// Example: <c><example><see cref="IAPCore"/></example></c></description>, 
    /// </item>
    /// <item>
    /// <description>
    /// The manager class is responsible for the module implementation.
    /// It's located inside the module itself and overrides the core class.
    /// Example: <c><example>IAPManager</example></c>
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    internal static class VoodooSauceCore
    {
        private const string TAG = "VoodooSauceCore";

#region Private common methods

        /// <summary>
        /// Try to find a child class to instantiate in priority, otherwise it instantiates the given class itself.
        /// </summary>
        /// <typeparam name="TModule">Module type (Module core class)</typeparam>
        /// <returns>The corresponding instance (never null).</returns>
        private static TModule InstantiateModule<TModule>()
            where TModule : class, IModule, new() => InstantiateChildClass<TModule>() ??
                                                           (TModule) Activator.CreateInstance(typeof(TModule));

        /// <summary>
        /// Return the instance of the module.
        /// If the optional parameter <c>logWarning</c> is true, log a warning if the module is not implemented in the current project.
        /// </summary>
        /// <param name="moduleInstance">The module instance.</param>
        /// <param name="logWarning">Optional parameter. If it's true, a warning is logged if the module isn't implemented in the project.</param>
        /// <typeparam name="TModule">Module class type.</typeparam>
        /// <returns>The module instance (never null).</returns>
        private static TModule GetModuleInstance<TModule>(TModule moduleInstance, bool logWarning)
            where TModule : class, IModule, new()
        {
            if (logWarning)
            {
                Type moduleType = moduleInstance.GetType();
                if (moduleType.BaseType == null || moduleType.BaseType == typeof(object))
                {
                    VoodooLog.LogWarning(Module.COMMON, TAG,
                        moduleType.Name + " module is not available/implemented.");
                }
            }

            return moduleInstance;
        }

        /// <summary>
        /// Checks and returns the first child class encountered.
        /// Logs an error if more than one child class is found.
        /// </summary>
        /// <typeparam name="TParentClass">A child class of this type is searched.</typeparam>
        /// <returns>The first child class encountered, null otherwise.</returns>
        [CanBeNull]
        private static TParentClass InstantiateChildClass<TParentClass>() where TParentClass : class
        {
            Type parentType = typeof(TParentClass);
            TParentClass firstChildClass = null;
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
            {
                // The current type is checked...
                if (parentType == type || !parentType.IsAssignableFrom(type) ||
                    type.IsInterface || type.IsAbstract)
                {
                    // ... and it's not corresponding.
                    continue;
                }

                // ... and it's what we are looking for.
                if (firstChildClass == null)
                {
                    // If this is the first encountered child class, we return it.
                    firstChildClass = (TParentClass) Activator.CreateInstance(type);
                }
                else
                {
                    // But if a second child class is also found, an error is logged.
                    VoodooLog.LogError(Module.COMMON, TAG,
                        "More than one child class has been found for " + parentType.Name);
                    break;
                }
            }

            return firstChildClass;
        }

        static VoodooSauceCore()
        {
            _privacy = InstantiateModule<PrivacyCore>();
            _crashReport = InstantiateModule<CrashReportCore>(); 
            _iap = InstantiateModule<IAPCore>();
        }
        
#endregion
        
        public static void Initialize(VoodooSauceBehaviour voodooSauceBehaviour)
        { 
            _voodooSauceBehaviour = voodooSauceBehaviour;
        }

        public static List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings) => _iap.IntegrationCheck(settings);

        private static readonly IAPCore _iap;

        internal static IAPCore GetInAppPurchase(bool logWarning = false) => GetModuleInstance(_iap, logWarning);
        
        public static Coroutine StartCoroutine(IEnumerator routine) => _voodooSauceBehaviour.StartCoroutine(routine);

        public static void StopCoroutine(IEnumerator routine) => _voodooSauceBehaviour.StopCoroutine(routine);
        public static void StopCoroutine(Coroutine coroutine) => _voodooSauceBehaviour.StopCoroutine(coroutine);

        private static readonly PrivacyCore _privacy;
        private static readonly CrashReportCore _crashReport;
        private static readonly List<IModule> _moduleList = new List<IModule>();
        private static VoodooSauceBehaviour _voodooSauceBehaviour;

        internal static PrivacyCore GetPrivacy(bool logWarning = false) => GetModuleInstance(_privacy, logWarning);
        internal static CrashReportCore GetCrashReport(bool logWarning = false) => GetModuleInstance(_crashReport, logWarning);

        internal static bool IsPremiumOrIAPSubscribed() =>
            VoodooSauce.IsPremium() || GetInAppPurchase().IsSubscribedProduct();
    }
}
