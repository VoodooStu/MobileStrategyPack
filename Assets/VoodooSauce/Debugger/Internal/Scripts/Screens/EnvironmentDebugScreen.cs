using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Tune.Core;

namespace Voodoo.Sauce.Debugger
{
    public class EnvironmentDebugScreen : Screen, IConditionalScreen
    {
        private const string NEW_SERVER_MESSAGE =
            "Your {0} server is set to {1}, restart the application to apply the modifications.";
        
        private const string NEW_VOODOO_TUNE_VERSION_MESSAGE =
            "Your VoodooTune version is set to {0}, restart the application to apply the modifications.";
        
        [SerializeField] private DebugPopup _debugPopup;

        private readonly Dictionary<EnvironmentSettings.Server, string> _server =
            new Dictionary<EnvironmentSettings.Server, string>
        {
            {EnvironmentSettings.Server.Tech, "Production"},
            {EnvironmentSettings.Server.Staging, "Staging"},
            {EnvironmentSettings.Server.Dev, "Development"}
        };

        private Server _voodooTuneServer;
        private Dictionary<Server, DebugToggleButton> _vtToggle =
            new Dictionary<Server, DebugToggleButton>();
        
        private Status _voodooTuneVersion;
        private Dictionary<Status, DebugToggleButton> _versionToggle =
            new Dictionary<Status, DebugToggleButton>();

        private EnvironmentSettings.Server _analyticsServer;
        private Dictionary<EnvironmentSettings.Server, DebugToggleButton> _analyticsToggle
            = new Dictionary<EnvironmentSettings.Server, DebugToggleButton>();
        
        public bool CanDisplay
        {
            get
            {
                var settings = VoodooSettings.Load();
                return settings.UseVoodooAnalytics || settings.UseRemoteConfig;
            }
        }

        private string VtNewVersion => string.Format(NEW_VOODOO_TUNE_VERSION_MESSAGE, VoodooTunePersistentData.SavedStatusToName[_voodooTuneVersion]);

        private void Start()
        {
            VoodooSettings settings = VoodooSettings.Load();
            InitProxyServerSection(settings);
            InitVoodooTune(settings);
            InitAnalytics(settings);
        }

        private void InitProxyServerSection(VoodooSettings settings)
        {
            if (settings.UseVoodooAnalytics) {
                InputField("Proxy Server", "Enter proxy server...", EnvironmentSettings.SaveProxyServer, EnvironmentSettings.GetProxyServer);
            }
        }

        private void InitVoodooTune(VoodooSettings settings)
        {
            if (settings.UseRemoteConfig == false)
            {
                return;
            }

            OpenFoldout("VoodooTune");
            InitVoodooTuneServerButtons();
            InitVoodooTuneVersionButtons();
            CloseFoldout();
        }

        private void InitVoodooTuneServerButtons()
        {
            _voodooTuneServer = VoodooTunePersistentData.SavedServer;
            foreach (Server value in Enum.GetValues(typeof(Server))) {
                var serverName = VoodooTunePersistentData.SavedServerDisplayNames[(int)value];
                _vtToggle.Add(value, Toggle(serverName, value == _voodooTuneServer, isOn => VoodooTuneServerSwitch(isOn, value)));
            }

            Label("Changing the server will affect remote config and remote config debugger.");
        }

        private void InitVoodooTuneVersionButtons()
        {
            _voodooTuneVersion = VoodooTunePersistentData.SavedStatus;
            // We don't want to show 'history'
            for (var i = 0; i < 2; i++) {
                var status = (Status)i;
                var statusName = VoodooTunePersistentData.SavedStatusToName[status];
                _versionToggle.Add(status, Toggle(statusName, status == _voodooTuneVersion, isOn => VoodooTuneSwitchVersion(isOn, status)));
            }

            Label("Changing the version will affect the remote config debugger.");
        }

        private void VoodooTuneServerSwitch(bool isOn, Server server)
        {
            bool shouldReturn = server == _voodooTuneServer || isOn == false;
            if (shouldReturn)
            {
                if (server == _voodooTuneServer && isOn == false)
                {
                    _vtToggle[server].SetValue(true);
                }

                return;
            }

            var former = _voodooTuneServer;
            _voodooTuneServer = server;

            _vtToggle[former].SetValue(false);
            _vtToggle[server].SetValue(true);

            VoodooTunePersistentData.SavedServer = server;

            string status = VoodooTunePersistentData.SavedStatusToName[_voodooTuneVersion];
            string message = string.Format(NEW_SERVER_MESSAGE, status, _voodooTuneServer);
            DisplayPopup(message);
        }

        private void VoodooTuneSwitchVersion(bool isOn, Status version)
        {
            bool shouldReturn = version == _voodooTuneVersion || isOn == false;
            if (shouldReturn)
            {
                if (version == _voodooTuneVersion && isOn == false)
                {
                    _versionToggle[version].SetValue(true);
                }

                return;
            }

            var former = _voodooTuneVersion;
            _voodooTuneVersion = version;

            _versionToggle[former].SetValue(false);
            _versionToggle[version].SetValue(true);

            VoodooTunePersistentData.SavedStatus = version;
            DisplayPopup(VtNewVersion);
        }

        private void InitAnalytics(VoodooSettings settings)
        {
            if (settings.UseVoodooAnalytics == false)
            {
                return;
            }

            OpenFoldout("Analytics");
            InitAnalyticsServerButtons();
            CloseFoldout();
        }

        private void InitAnalyticsServerButtons()
        {
            _analyticsServer = EnvironmentSettings.GetAnalyticsServer();
            foreach (EnvironmentSettings.Server value in Enum.GetValues(typeof(EnvironmentSettings.Server)))
            {
                _analyticsToggle.Add(value, Toggle(_server[value], value == _analyticsServer, isOn => AnalyticsServerSwitch(isOn, value)));
            }

            Label("Changing the server will affect analytics.");
        }

        private void AnalyticsServerSwitch(bool isOn, EnvironmentSettings.Server server)
        {
            bool shouldReturn = server == _analyticsServer || isOn == false;
            if (shouldReturn)
            {
                if (server == _analyticsServer && isOn == false)
                {
                    _analyticsToggle[server].SetValue(true);
                }

                return;
            }

            var former = _analyticsServer;
            _analyticsServer = server;

            _analyticsToggle[former].SetValue(false);
            _analyticsToggle[server].SetValue(true);

            EnvironmentSettings.SaveAnalyticsServer(server);
            
            string message = string.Format(NEW_SERVER_MESSAGE, _server[server].ToLower(), _analyticsServer);
            DisplayPopup(message);
        }

        private void DisplayPopup(string message) 
        {
            _debugPopup.Initialize(message, () => { _debugPopup.gameObject.SetActive(false); });
            _debugPopup.gameObject.SetActive(true);
        }
    }
}