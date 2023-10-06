using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Editor
{
    public class VoodooSauceIntegrationCheck : IIntegrationCheck
    {
        private static readonly string[] _validUnityVersions =
        {
            "2020.3",
            "2021.1",
            "2021.2",
            "2021.3",
            "2022.1",
            "2022.2",
            "2022.3"
        };
        private static readonly Version _minimumUnityVersion = Version.Parse(_validUnityVersions[0]);

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var result = new List<IntegrationCheckMessage>();
            var unityCheckResult = IsValidUnityVersion();
            if(unityCheckResult != null) result.Add(unityCheckResult);
            return result;
        }

        /// <summary>
        /// Check if the current Unity version is compatible with the VoodooSauce.
        /// </summary>
        /// <returns>Return integration warning message if Unity version is not correct, otherwise return null</returns>
        private static IntegrationCheckMessage IsValidUnityVersion()
        {
            string pattern = @"[a-z]+\d+.*";
            string curVersion = Regex.Replace(Application.unityVersion, pattern, "");
            
            Version currentUnityVersion = Version.Parse(curVersion);
            
            if (currentUnityVersion < _minimumUnityVersion)
            {
                return new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR,
                    $"This Unity version is too old, please use at least {_minimumUnityVersion}"); 
            }

            //If unity version is valid return null
            if (_validUnityVersions.Select(unityVersion => Regex.Match(curVersion, unityVersion)).Any(m => m.Success))
            {
                return null;
            }

            return new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING,
                "Your UnityVersion has not been tested with VoodooSauce.  We currently support Unity " +
                GetSupportedVersionsString());
        }
        
        private static string GetSupportedVersionsString()
        {
            return string.Join(", ", _validUnityVersions.Select(v => $"{v}.x"));
        }
    }
}