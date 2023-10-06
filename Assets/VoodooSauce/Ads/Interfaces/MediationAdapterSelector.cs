using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Ads.FakeMediation;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads
{
    internal static class MediationAdapterSelector
    {
        private const string TAG = "MediationAdapterSelector";

        internal static IMediationAdapter CreateMediationInstance()
        {
            List<Type> mediationClasses = GetMediationAdaptersClasses();

            if (mediationClasses.Count == 0) {
                string interfaceName = typeof(IMediationAdapter).FullName;
                throw new NotSupportedException($"You need to have at least one class that inherits from {interfaceName}.");
            }
                    
            SelectMediationIfNeeded(mediationClasses);
            
            int mediationIndex = GetMediationChoice();
            Type mediationType = mediationClasses[mediationIndex];
            return (IMediationAdapter) Activator.CreateInstance(mediationType);
        }

        private static void SelectMediationIfNeeded(List<Type> mediationClasses)
        {
            if (!ShouldResetMediationChoice()) {
                return;
            }
            
            VoodooLog.LogDebug(Module.ADS, TAG, "Should reset mediation choice");
            // assign the user to one of the mediations
            var mediationChoice = (int) (UnityEngine.Random.value * mediationClasses.Count);

            // save the mediation choice
            SetMediationChoice(mediationChoice);
        }

        private static bool ShouldResetMediationChoice()
        {
            bool hasMediationTypeKey = PlayerPrefs.HasKey(AdsConstants.MEDIATION_TYPE);
            int mediationTypeIndex = PlayerPrefs.GetInt(AdsConstants.MEDIATION_TYPE, -1);
            VoodooLog.LogDebug(Module.ADS, TAG, $"Mediation choice made: {hasMediationTypeKey} (value: {mediationTypeIndex})");
            
            if (!PlayerPrefs.HasKey(AdsConstants.MEDIATION_TYPE)) return true;

            List<Type> mediationClasses = GetMediationAdaptersClasses();
            VoodooLog.LogDebug(Module.ADS, TAG, $"Mediation choice: {mediationTypeIndex}");

            // when the index is out of bounds of the mediations list, it means the user should be reassigned 
            return mediationTypeIndex < 0 || mediationTypeIndex >= mediationClasses.Count;
        }

        private static int GetMediationChoice()
        {
            List<Type> mediationClasses = GetMediationAdaptersClasses();
            int index = PlayerPrefs.GetInt(AdsConstants.MEDIATION_TYPE, -1);

            return index < 0 || index >= mediationClasses.Count ? 0 : index;
        }

        private static void SetMediationChoice(int mediationChoice)
        {
            VoodooLog.LogDebug(Module.ADS, TAG, $"Mediation set to {mediationChoice}");
            PlayerPrefs.SetInt(AdsConstants.MEDIATION_TYPE, mediationChoice);
        }

        private static List<Type> GetMediationAdaptersClasses()
        {
            Type interfaceType = typeof(IMediationAdapter);

            List<Type> mediationClasses = AssembliesUtils.GetTypes(interfaceType);
            mediationClasses = (from mediationClass in mediationClasses
                                where !string.Equals(mediationClass.Name, nameof(FakeMediationAdapter))
                                select mediationClass).ToList();

            foreach (Type type in mediationClasses) {
                VoodooLog.LogDebug(Module.ADS, TAG, $"MediationType: {type} loaded");
            }

            return mediationClasses;
        }
    }
}