using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Tune.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    internal static class DebugVTManager
    {
        private const string TAG = "DebugVTManager";
        
        private static readonly List<ABTestState> DebuggerAbTestStates = new List<ABTestState> { ABTestState.draft, ABTestState.pending, ABTestState.published, ABTestState.terminating };

        /// <summary>
        /// This represents the VoodooTune debug configuration currently applied.
        /// This configuration is used to initialize the VoodooTune module.
        /// </summary>
        internal static DebugVTConfiguration CurrentDebugConfiguration { get; private set; }

        /// <summary>
        /// This represents the current draft debug configuration if any.
        /// This is basically created when a new configuration is selected in the VoodooTune debugger.
        /// After a reboot of the game, this configuration will be applied to the initialization of VoodooTune.
        /// </summary>
        [CanBeNull]
        internal static DebugVTConfiguration DraftDebugConfiguration { get; private set; }

        /// <summary>
        /// This property is true if a debug draft configuration is currently existing.
        /// </summary>
        internal static bool IsDraftDebugConfiguration => DraftDebugConfiguration != null && !DraftDebugConfiguration.Equals(CurrentDebugConfiguration);

        internal delegate void OnDebugConfigurationChange();

        /// <summary>
        /// This event is triggered when the configuration is changed.
        /// </summary>
        internal static event OnDebugConfigurationChange OnDebugConfigurationChangeEvent;

        /// <summary>
        /// This contains all the information loaded by <see cref="LoadDebugConfigurations"/>.
        /// </summary>
        private static VersionMetadata _version;

        /// <summary>
        /// This contains all the abtests loaded by <see cref="LoadDebugConfigurations"/>.
        /// </summary>
        private static IReadOnlyList<ABTest> _abTests;

        /// <summary>
        /// This contains all the abtests loaded by <see cref="LoadDebugConfigurations"/>.
        /// </summary>
        private static IReadOnlyList<Segment> _segments;

        /// <summary>
        /// This contains all the abtests loaded by <see cref="LoadDebugConfigurations"/>.
        /// </summary>
        private static IReadOnlyList<Sandbox> _sandboxes;

        internal static void Initialize()
        {
            CurrentDebugConfiguration = DebugVTConfiguration.GetFromPlayerPrefs();
            DraftDebugConfiguration = null;
            OnDebugConfigurationChangeEvent += OnDebugConfigurationUpdate;
        }

        /// <summary>
        /// Check if a custom debug configuration is currently applied to VoodooTune.
        /// </summary>
        /// <returns>True if a custom application is applied, false if the VoodooTune behavior is the default one.</returns>
        /// <exception cref="NullReferenceException">This exception is thrown if <see cref="DebugVTManager"/> isn't initialized yet.</exception>
        internal static bool HasDebugBehaviorApplied()
        {
            if (CurrentDebugConfiguration == null)
            {
                throw new NullReferenceException("DebugVTManager.Initialize() should be called before this.");
            }

            return !CurrentDebugConfiguration.IsDefaultConfiguration;
        }

        /// <summary>
        /// Remove the <see cref="DraftDebugConfiguration"/> if any.
        /// </summary>
        internal static void RemoveDebugDraftConfiguration()
        {
            DraftDebugConfiguration = null;
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        /// <summary>
        /// This method is called everytime the debug configuration is modified.
        /// This synchronizes the <see cref="PlayerPrefs"/> according to the current draft configuration.
        /// </summary>
        private static void OnDebugConfigurationUpdate()
        {
            if (DraftDebugConfiguration != null)
            {
                DraftDebugConfiguration.PersistOnPlayerPrefs();
            }
            else
            {
                CurrentDebugConfiguration.PersistOnPlayerPrefs();
            }
        }

        /// <summary>
        /// Select a sandbox.
        /// </summary>
        /// <param name="sandbox">The selected sandbox. It can be null.</param>
        /// <exception cref="NullReferenceException">This exception is thrown if no draft configuration has been created.
        /// You can use the <see cref="NewDebugDraftConfiguration"/> method to create a new one.</exception>
        internal static void SelectDebugSandbox(Sandbox sandbox)
        {
            if (DraftDebugConfiguration == null)
            {
                throw new NullReferenceException("Can't select/deselect a sandbox because no draft configuration has been created.");
            }

            DraftDebugConfiguration = new DebugVTConfiguration
            {
                SelectedSandbox = sandbox
            };
            
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        /// <summary>
        /// Add a cohort to the selected ones.
        /// </summary>
        /// <param name="cohort">The selected cohort.</param>
        /// <param name="abTest">The AB test of the selected cohort.</param>
        /// <exception cref="NullReferenceException">This exception is thrown if no draft configuration has been created.
        /// You can use the <see cref="NewDebugDraftConfiguration"/> method to create a new one.</exception>
        public static bool AddCohort(Cohort cohort, ABTest abTest)
        {
            if (DraftDebugConfiguration == null)
            {
                throw new NullReferenceException("Can't select a cohort because no draft configuration has been created.");
            }

            if (DraftDebugConfiguration.SelectedCohorts.Contains(cohort))
            {
                return false;
            }

            DraftDebugConfiguration.SelectedSandbox = null;
            
            DraftDebugConfiguration.SelectedAbTests.Add(abTest);
            DraftDebugConfiguration.SelectedCohorts.Add(cohort);
            DraftDebugConfiguration.VersionId = _version.Id;
            OnDebugConfigurationChangeEvent?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Remove a cohort from the selected ones.
        /// </summary>
        /// <param name="cohort">The selected cohort.</param>
        /// <param name="abTest">The AB test of the selected cohort.</param>
        /// <exception cref="NullReferenceException">This exception is thrown if no draft configuration has been created.
        /// You can use the <see cref="NewDebugDraftConfiguration"/> method to create a new one.</exception>
        public static bool RemoveCohort(Cohort cohort, ABTest abTest)
        {
            if (DraftDebugConfiguration == null)
            {
                throw new NullReferenceException("Can't select a cohort because no draft configuration has been created.");
            }
            
            if (DraftDebugConfiguration.SelectedCohorts.Contains(cohort) == false)
            {
                return false;
            }

            DraftDebugConfiguration.SelectedSandbox = null;

            DraftDebugConfiguration.SelectedAbTests.Remove(abTest);
            DraftDebugConfiguration.SelectedCohorts.Remove(cohort);
            OnDebugConfigurationChangeEvent?.Invoke();
            return true;

        }

        /// <summary>
        /// Clear the selected cohorts.
        /// </summary>
        public static void ClearCohorts()
        {
            if (DraftDebugConfiguration == null)
            {
                return;
            }
            
            DraftDebugConfiguration.SelectedAbTests.Clear();
            DraftDebugConfiguration.SelectedCohorts.Clear();
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        public static void ClearSandboxes()
        {
            if (DraftDebugConfiguration == null)
            {
                return;
            }
            
            DraftDebugConfiguration.SelectedSandbox = null;
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        /// <summary>
        /// Add a segment to the selected ones.
        /// </summary>
        /// <param name="segment">The selected segment. It can be null.</param>
        /// <exception cref="NullReferenceException">This exception is thrown if no draft configuration has been created.
        /// You can use the <see cref="NewDebugDraftConfiguration"/> method to create a new one.</exception>
        public static bool AddDebugSegment(Segment segment)
        {
            if (DraftDebugConfiguration == null)
            {
                throw new NullReferenceException("Can't select a segment because no draft configuration has been created.");
            }

            if (segment == null || DraftDebugConfiguration.SelectedSegments.Contains(segment))
            {
                return false;
            }

            DraftDebugConfiguration.SelectedSandbox = null;
            
            DraftDebugConfiguration.SelectedSegments.Add(segment);
            DraftDebugConfiguration.VersionId = _version.Id;
            
            OnDebugConfigurationChangeEvent?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove a segment from the selected ones.
        /// </summary>
        /// <param name="segment">The selected segment. It can be null.</param>
        /// <exception cref="NullReferenceException">This exception is thrown if no draft configuration has been created.
        /// You can use the <see cref="NewDebugDraftConfiguration"/> method to create a new one.</exception>
        public static bool RemoveDebugSegment(Segment segment)
        {
            if (DraftDebugConfiguration == null)
            {
                throw new NullReferenceException("Can't select a segment because no draft configuration has been created.");
            }

            if (segment == null || DraftDebugConfiguration.SelectedSegments.Contains(segment) == false)
            {
                return false;
            }

            DraftDebugConfiguration.SelectedSandbox = null;
            
            DraftDebugConfiguration.SelectedSegments.Remove(segment);
            OnDebugConfigurationChangeEvent?.Invoke();
            return true;
        }

        /// <summary>
        /// Clear the selected segments.
        /// </summary>
        public static void ClearDebugSegments()
        {
            if (DraftDebugConfiguration == null)
            {
                return;
            }
            
            DraftDebugConfiguration.SelectedSegments.Clear();
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        /// <summary>
        /// Load the available debug configurations with a HTTP request.
        /// This method must be called from an iOS or an Android project.
        /// </summary>
        /// <param name="onSuccess">If the configurations has been correctly loaded.</param>
        /// <param name="onError">If an error has occured when trying to load debug configurations, an error message is specified.</param>
        /// <exception cref="NotImplementedException">This exception is thrown if the method is not called from an Android or an iOS project.</exception>
        internal static async void LoadDebugConfigurations(Action onSuccess, Action<string> onError)
        {
            if (!PlatformUtils.UNITY_IOS && !PlatformUtils.UNITY_ANDROID && !PlatformUtils.UNITY_EDITOR)
            {
                throw new NotImplementedException("This method only works on an Android/iOS project.");
            }

            string error = null;

            try
            {
                VoodooTuneClient client = new VoodooTuneClient();
                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, $"LoadDebugConfigurations API call for {Application.identifier} ({VoodooTunePersistentData.SavedStatus.ToString()})");
                
                _version = await client.App.Version.Get(Application.identifier, VoodooTunePersistentData.SavedStatus.ToString());
                VoodooTunePersistentData.SavedAppId = _version.AppId;
                
                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, "LoadDebugConfigurations Version success");
                if (VoodooTunePersistentData.SavedAppId == null)
                {
                    onError.Invoke("The AppId returned by the service is null.");
                    return;
                }

                try
                {
                    _abTests = await client.App.Version.AbTest.GetAll(_version.AppId, _version.Id, DebuggerAbTestStates);
                    _segments = await client.App.Version.Segment.GetAll(_version.AppId, _version.Id);
                    _sandboxes = await client.App.Sandbox.GetAll(_version.AppId);
                }
                catch (Exception e) {
                    error = e.Message;
                }
            }
            catch (Exception e) {
                error = e.Message;
            }

            if (!string.IsNullOrEmpty(error)) {
                VoodooLog.LogError(Module.VOODOO_TUNE, TAG, "LoadDebugConfigurations API error");
                onError?.Invoke(error);
            } else {
                VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG, "LoadDebugConfigurations API success");
                onSuccess?.Invoke();
            }
        }

        /// <summary>
        /// Retrieve the list of sandboxes available for debugging purposes if the configurations have been loaded (<see cref="LoadDebugConfigurations"/>).
        /// </summary>
        /// <returns>List of sandboxes</returns>
        /// <exception cref="NullReferenceException">If no configuration has been loaded.</exception>
        internal static IEnumerable<Sandbox> GetDebugSandboxes()
        {
            if (_sandboxes == null)
            {
                throw new NullReferenceException("A debug configuration should be loaded before calling this method.");
            }
        
            return _sandboxes;
        }

        /// <summary>
        /// Retrieve the list of ABTests available for debugging purposes if the configurations have been loaded (<see cref="LoadDebugConfigurations"/>).
        /// </summary>
        /// <returns>List of ABTests</returns>
        /// <exception cref="NullReferenceException">If no configuration has been loaded.</exception>
        internal static IEnumerable<ABTest> GetDebugAbTests()
        {
            if (_abTests == null)
            {
                throw new NullReferenceException("A debug configuration should be loaded before calling this method.");
            }

            return _abTests;
        }

        /// <summary>
        /// Retrieve the list of segments available for debugging purposes if the configurations have been loaded (<see cref="LoadDebugConfigurations"/>).
        /// </summary>
        /// <returns>List of segments</returns>
        /// <exception cref="NullReferenceException">If no configuration has been loaded.</exception>
        internal static IEnumerable<Segment> GetDebugSegments()
        {
            if (_segments == null)
            {
                throw new NullReferenceException("A debug configuration should be loaded before calling this method.");
            }

            return _segments;
        }

        /// <summary>
        /// Create a debug draft configuration that is, by default, a copy of the current configuration.
        /// You can choose the configuration element that you want to copy using the flag.
        /// </summary>
        internal static void NewDebugDraftConfiguration()
        {
            DraftDebugConfiguration = new DebugVTConfiguration();
            
            if (CurrentDebugConfiguration != null)
            {
                DraftDebugConfiguration.SelectedAbTests = new List<ABTest>(CurrentDebugConfiguration.SelectedAbTests);
                DraftDebugConfiguration.SelectedCohorts = new List<Cohort>(CurrentDebugConfiguration.SelectedCohorts);
                DraftDebugConfiguration.SelectedSegments = new List<Segment>(CurrentDebugConfiguration.SelectedSegments);
                DraftDebugConfiguration.SelectedSandbox = CurrentDebugConfiguration.SelectedSandbox;
                DraftDebugConfiguration.VersionId = CurrentDebugConfiguration.VersionId;
            }
            
            OnDebugConfigurationChangeEvent?.Invoke();
        }

        /// <summary>
        /// Create a debug draft configuration with the default behavior of VoodooTune.
        /// (It removes any sandbox/segment and/or cohort selection)
        /// </summary>
        internal static void ResetDebugDraftConfiguration()
        {
            DraftDebugConfiguration = new DebugVTConfiguration();
            OnDebugConfigurationChangeEvent?.Invoke();
        }
    }
}