using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Debugger
{
    public class LogsOptionDebugScreen : Screen
    {
        [SerializeField] private Toggle enableDebugLogsToggle;
        [SerializeField] private GameObject restartLogsView;
        [SerializeField] private RectTransform logFilterContainer;
        [SerializeField] private GameObject logFilterParent;
        
        [Header("Prefabs"),SerializeField] private Toggle logFilterPrefab;
        [SerializeField] private Text logHeaderTextPrefab;

        private bool _originalDebugLogsEnabled;
        
        private Toggle[] _logFilterToggles;

        private void Awake()
        {
            enableDebugLogsToggle.onValueChanged.AddListener(OnEnableDebugLogsToggled);

            enableDebugLogsToggle.isOn = VoodooLog.IsDebugLogsEnabled;
            _originalDebugLogsEnabled = VoodooLog.IsDebugLogsEnabled;

            restartLogsView.SetActive(false);
        }

        private void OnDestroy()
        {
            enableDebugLogsToggle.onValueChanged.RemoveListener(OnEnableDebugLogsToggled);
        }

        private void Start()
        {
            CreateLogModuleToggles();

            Destroy(logHeaderTextPrefab.gameObject);
            Destroy(logFilterPrefab.gameObject);
        }

        private void CreateLogModuleToggles()
        {
            string[] logModuleNames = GetEnumValues(typeof(Module));
            
            Text logModuleHeader = Instantiate(logHeaderTextPrefab, logFilterContainer);
            logModuleHeader.text = "Log Module";
            
            _logFilterToggles = new Toggle[logModuleNames.Length - 1];
            for (var i = 1; i < logModuleNames.Length-1; i++) 
            {
                _logFilterToggles[i - 1] = CreateModuleToggle(i, logModuleNames[i]);
            }
        }

        private Toggle CreateModuleToggle(int index, string moduleName)
        {
            Toggle toggle = Instantiate(logFilterPrefab, logFilterContainer);
            var label = toggle.GetComponentInChildren<Text>();
            label.text = FormatModuleName(moduleName);
            
            int filter = VoodooLog.ModuleFilter;
            int value = 1 << (index - 1);
            toggle.isOn = (filter & value) > 0;
            toggle.onValueChanged.AddListener(b => OnLogModuleChanged(value));
            return toggle;
        }

        private string FormatModuleName(string module)
        {
            string str = module.ToLower().Replace('_', ' ');
            str = str.Substring(0, 1).ToUpper() + str.Substring(1);
            return str;
        }

        private void OnLogModuleChanged(int value)
        {
            VoodooLog.ToggleModule(value);
        }

        private void OnEnableDebugLogsToggled(bool enableLogs)
        {
            VoodooLog.EnableDebugLogs(enableLogs);
            
            restartLogsView.SetActive(enableLogs != _originalDebugLogsEnabled);
            logFilterParent.SetActive(enableLogs);
        }

        private string[] GetEnumValues(Type enumType)
        {
            return Enum.GetValues(enumType).OfType<object>().Select(o => o.ToString()).ToArray();
        }
    }
}