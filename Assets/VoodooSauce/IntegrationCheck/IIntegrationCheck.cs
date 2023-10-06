using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.IntegrationCheck
{
    /// <summary>
    /// This interface can be used to add checks to make for the VoodooSettings "Run Verification Test" feature.
    /// </summary>
    public interface IIntegrationCheck
    {
        /// <summary>
        /// This method will be called to check if everything is well integrated.
        /// </summary>
        /// <param name="settings">Current voodoo settings</param>
        /// <returns>List of messages you want to display to the developer. Errors and Warnings can be displayed.</returns>
        List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings);
    }

    /// <summary>
    /// Those messages are sent if something is wrong during the "Run verification test".
    /// Those messages can be warnings or errors.
    /// </summary>
    public class IntegrationCheckMessage
    {
        public enum Type
        {
            WARNING,
            ERROR
        }

        public readonly Type type;
        public string Description {
            get {
                if (_description != null) {
                    if (parameters != null) {
                        try {
                            return string.Format(_description, parameters);
                        } catch (FormatException) {
                            Debug.LogError("Error when formatting this IntegrationCheckMessage.");
                            return "";
                        }
                    } else {
                        return _description;
                    }
                } else {
                    Debug.LogError("No description assigned to this IntegrationCheckMessage.");
                    return "";
                }
            }
        }
        /// <summary>
        /// True if this message need to display a VoodooSettings shortcut button next to it.
        /// </summary>
        public readonly bool isBackToSettingsBtnDisplayed;

        private readonly string _description;
        [CanBeNull]
        private readonly string[] parameters;

        /// <summary>
        /// Create a message.
        /// </summary>
        /// <param name="type">Type of this message. It can be a warning or an error</param>
        /// <param name="description">Description of the message. This text is displayed on a Debug.Log and on the Integration check window.</param>
        /// <param name="parameters">These parameters are included into the final message. This is optional.</param>
        /// <param name="isBackToSettingsBtnDisplayed">If the VoodooSettings shortcut button should be displayed.</param>
        public IntegrationCheckMessage(Type type, string description, string[] parameters = null, bool isBackToSettingsBtnDisplayed = false)
        {
            this.type = type;
            _description = description;
            this.parameters = parameters;
            this.isBackToSettingsBtnDisplayed = isBackToSettingsBtnDisplayed;
        }
    }
}