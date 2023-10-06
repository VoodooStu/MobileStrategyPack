using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Tune.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneConfigurationDetailsScreen : Screen
    {
        [Header("Configuration Details"), SerializeField] private Text configurationDetailsLabel;
        [SerializeField] private Button copyButton;
        
        private static readonly Regex ClassNameRegex = new Regex("\\.([^\\.]*)$");

        private void OnEnable()
        {
            copyButton.onClick.AddListener(CopyConfigurationDetails);
        }

        private void OnDisable()
        {
            copyButton.onClick.RemoveListener(CopyConfigurationDetails);
        }

        private void Start()
        {
            configurationDetailsLabel.text = "";
            foreach (KeyValuePair<string, string> line in VoodooTuneManager.GetItemsJson())
            {
                configurationDetailsLabel.text += FormatClassName(line.Key) + FormatVariables(line.Value) + '\n';
            }
        }

        private static string FormatClassName(string value)
        {
            string className = ClassNameRegex.Match(value).Groups[1].Value;
            var namespaceLine = "";
            if (string.IsNullOrEmpty(className))
            {
                className = value;
            }
            else
            {
                string nameSpace = value.Substring(0, value.Length - className.Length - 1);
                namespaceLine = "<i><size=35><color=#4D4D4D>" + nameSpace + "</color></size></i>\n";
            }

            return "\n<b><size=45>" + className + "</size></b>\n" + namespaceLine + '\n';
        }

        private static string FormatVariables(string value)
        {
            value = '\t' + value.Replace("[", "")
                                .Replace("]", "")
                                .Replace("{", "")
                                .Replace("}", "")
                                .Replace("\"", "")
                                .Replace(",", "\n\t");
            value = Regex.Replace(value, "(.*):(.*)", "$1: $2");
            value = Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
            return value;
        }

        public void CopyConfigurationDetails()
        {
            VoodooTunePersistentData.SavedConfig.CopyToClipboard();
        }
    }
}