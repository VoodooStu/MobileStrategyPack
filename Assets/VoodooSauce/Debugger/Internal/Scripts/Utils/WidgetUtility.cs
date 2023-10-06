using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Debugger.Widgets;

namespace Voodoo.Sauce.Debugger
{
    public static class WidgetUtility
    {
        private const string BANK_PATH = "VoodooDebuggerWidgetBank";

        private static DebuggerCanvas _debuggerCanvasPrefab;
        private static Dictionary<string, Widget> _bank = new Dictionary<string, Widget>();

        static WidgetUtility() 
        {
            var bank = Resources.Load<WidgetBank>(BANK_PATH);
            _debuggerCanvasPrefab = bank.debuggerPrefab;
            foreach (var widget in bank.widgets) {
                var type = widget.GetType();
                if (type.FullName != null) {
                    _bank.Add(type.FullName, widget);
                }
            }
        }

        public static T Get<T>(string key) where T : Widget
        {
            if (!_bank.ContainsKey(key)) {
                throw new KeyNotFoundException($"Widget was not found inside bank : '{key}'");
            }
            return (T)_bank[key];   
        }

        public static Widget Get(string key) => _bank[key];

        public static DebuggerCanvas InstantiateDebugger() => GameObject.Instantiate(_debuggerCanvasPrefab);

        public static T InstanceOf<T>(Transform parent = null) where T : Widget
        {
            var prefab = Get<T>(typeof(T).FullName);
            return GameObject.Instantiate(prefab, parent);
        }

        public static DebugMenuItemButton MenuItem(string title, Sprite image, Color imageColor, Action callback, BadgeCounter counter = null, Transform parent = null) 
        {
            var menuButton = InstanceOf<DebugMenuItemButton>(parent);
            
            menuButton.SetText(title);
            menuButton.SetIcon(image, imageColor);
            menuButton.SetCallback(callback);
            menuButton.SetBadgeCounter(counter);

            return menuButton;
        }

        public static DebugHideableSection Foldout(string title, Transform parent = null)
        {
            var foldout = InstanceOf<DebugHideableSection>(parent);
            if (foldout == null)
            {
                return null;
            }

            foldout.GetComponentInChildren<Text>().text = title;

            return foldout;
        }

        public static DebugTextPair Label(Func<string> refreshFunc, Transform parent = null) 
            => CopyToClipboard(string.Empty, refreshFunc, null, false, parent);
        
        public static DebugTextPair Label(string text, Transform parent = null) 
            => CopyToClipboard(text, null, null, false, parent);
        
        public static DebugTextPair Label(string text, string value, Transform parent = null)
            => CopyToClipboard(text, () => value, null, false, parent);
        
        public static DebugTextPair Label(string text, Func<string> refreshFunc, Transform parent = null)
            => CopyToClipboard(text, refreshFunc, null, false, parent);

        public static DebugTextPair CopyToClipboard(string label, string value, Transform parent = null) 
            => CopyToClipboard(label, () => value, () => value, false, parent);
        
        public static DebugTextPair CopyToClipboard(string label, Func<string> refreshFunc, Transform parent = null)
            => CopyToClipboard(label, refreshFunc, refreshFunc, false, parent);

        public static DebugTextPair CopyToClipboard(string label, string value, Func<string> valueToCopy, Transform parent = null)
            => CopyToClipboard(label, () => value, valueToCopy, false, parent);

        public static DebugTextPair CopyToClipboard(string label,
                                                    Func<string> refreshFunc,
                                                    Func<string> valueToCopy,
                                                    bool isWarning = false,
                                                    Transform parent = null)
        {
            var textPair = InstanceOf<DebugTextPair>(parent);

            textPair.SetLabel(label);
            textPair.SetValue(refreshFunc?.Invoke());
            textPair.SetRefreshFunc(refreshFunc);
            
            if (isWarning)
            {
                textPair.SetStyle(Color.red, FontStyle.Bold);
            }

            if (valueToCopy == null)
            {
                textPair.ShowCopyButton(false);
            }
            else
            {
                textPair.SetValueToCopy(valueToCopy?.Invoke());
                textPair.SetRefreshToCopyFunc(valueToCopy);
            }

            return textPair;
        }

        public static void CreateSpace(int height = 10, Transform parent = null)
        {
            CreateSeparator(height, false, parent);
        }

        public static void CreateSeparator(int height = 10, Transform parent = null)
        {
            CreateSeparator(height, true, parent);
        }

        public static void CreateSeparator(int height, bool displayLine, Transform parent = null)
        {
            var separator = InstanceOf<DebugSeparator>(parent);
            separator.SetSize(height);
            separator.DisplayLine(displayLine);
        }

        public static DebugButtonWithInputField Button(string title, Action callback, Transform parent = null)
        {
            var btn = InstanceOf<DebugButtonWithInputField>(parent);
            btn.SetTitle(title);
            btn.SetButtonCallback(callback);
            btn.SetInputField(false);

            if (callback == null) {
                btn.SetEnable(false);
            }
            
            return btn;
        }

        public static DebugButtonWithInputField ButtonWithInput(string title, string placeholderText, Action<string> callback, Func<string> refreshFunc = null, Transform parent = null)
        {
            var btn = InstanceOf<DebugButtonWithInputField>(parent);
            btn.SetTitle(title);
            btn.SetButtonCallback(callback);
            btn.SetInputField(true, placeholderText);
            btn.SetRefreshFunc(refreshFunc);

            if (callback == null) {
                btn.SetEnable(false);
            }

            return btn;
        }
        
        public static DebugButtonWithInputField InputField(string title, string placeholderText, Action<string> inputCallback, Func<string> refreshFunc = null, Transform parent = null)
        {
            var btn = InstanceOf<DebugButtonWithInputField>(parent);
            btn.SetTitle(title);
            btn.SetInputCallback(inputCallback);
            btn.SetInputField(true, placeholderText);
            btn.SetRefreshFunc(refreshFunc);

            return btn;
        }

        public static DebugToggleButton Toggle(string title, bool value, Action<bool> callback, Transform parent = null)
        {
            var toggle = InstanceOf<DebugToggleButton>(parent);
            toggle.Initialize(title, value, callback);

            return toggle;
        }

        public static DebugRadioButton RadioButton(string label, Action<bool> callback, Transform parent)
        {
            var radioButton = InstanceOf<DebugRadioButton>(parent);
            radioButton.SetLabel(label);
            radioButton.SetCallback(callback);

            return radioButton;
        }

        public static DebugRowLabel RowLabel(string rowName, string value, Transform parent = null)
        {
            return RowLabel(rowName, new [] { value }, parent);
        }

        public static DebugRowLabel RowLabel(string rowName, string[] values, Transform parent = null)
        {
            var rowLabel = InstanceOf<DebugRowLabel>(parent);
            rowLabel.SetRowName(rowName);
            rowLabel.AddRows(values);
            return rowLabel;
        }

        public static DebugRadioGroup RadioGroup() => new DebugRadioGroup();

        public static DebugCheckboxButton CheckboxButton(string label, Action<bool> callback, Transform parent)
        {
            var checkboxBtn = InstanceOf<DebugCheckboxButton>(parent);
            checkboxBtn.SetLabel(label);
            checkboxBtn.SetCallback(callback);
            return checkboxBtn;
        }
    }
}
