using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Debugger.Widgets;
using Voodoo.Sauce.Internal.DebugScreen;

namespace Voodoo.Sauce.Debugger
{
    public partial class Screen : Widget, IComparable, IComparable<Screen>
    {
        [Header("Screen")]
        public Sprite image;
        public Color imageColor = Color.white;
        public string title;
        public int orderIndex;
        public Screen mainSubScreen;

        [SerializeField] Transform _parent;
        
        private DebugHideableSection _currentFoldout;
        private DebugRadioGroup _currentRadioGroup;

        private List<DebugRadioGroup> _radioGroups = new List<DebugRadioGroup>();
        private List<IDebugRefreshable> _refreshables = new List<IDebugRefreshable>();
        private List<Widget> _widgets = new List<Widget>();

        protected Transform Parent => _parent ? _parent : transform;

        public virtual BadgeCounter Counter() => null;

        public virtual void OnScreenShow() { }
        public virtual void OnScreenHide() { }

        protected void ClearDisplay()
        {
            foreach (Transform item in Parent)
            {
                Destroy(item.gameObject);
            }
        }

        public DebugHideableSection OpenFoldout(string foldoutTitle) => _currentFoldout = WidgetUtility.Foldout(foldoutTitle, Parent);
        
        public void CloseFoldout() => _currentFoldout = null;

        public DebugRadioGroup CreateRadioGroup() => _currentRadioGroup = WidgetUtility.RadioGroup();

        public void CloseRadioGroup()
        {
            if (_currentRadioGroup != null) {
                _radioGroups.Add(_currentRadioGroup);
                _currentRadioGroup = null;
            }
        }

        public void RefreshWidgets()
        {
            foreach (var widget in _refreshables)
            {
                widget.Refresh();
            }
        }
        
        public T Assign<T>(T widget)  where T : Widget
        {
            _widgets.Add(widget);
            
            if (widget is IDebugRefreshable refreshable)
            {
                _refreshables.Add(refreshable);
            }
            
            if (_currentFoldout == null)
            {
                return widget;
            }
            
            _currentFoldout.AddChild(widget.gameObject);

            return widget;
        }

        public List<T> GetAll<T>() where T : Widget
        {
            List<T> list = new List<T>();
            foreach (var obj in _widgets) {
                if (obj is T elt) {
                    list.Add(elt);
                }
            }
            return list;
        }

        public DebugMenuItemButton MenuItem(string title, Sprite image, Color imageColor, Action callback, BadgeCounter counter = null)
            => Assign(WidgetUtility.MenuItem(title, image, imageColor, callback, counter, Parent));

        public DebugTextPair Label(Func<string> refreshFunc) => Assign(WidgetUtility.Label(refreshFunc, Parent));

        public DebugTextPair Label(string text) => Assign(WidgetUtility.Label(text, Parent));

        public DebugTextPair Label(string text, string value) => Assign(WidgetUtility.Label(text, value, Parent));

        public DebugTextPair Label(string text, Func<string> refreshFunc) => Assign(WidgetUtility.Label(text, refreshFunc, Parent));

        public DebugTextPair Label(string label, Func<string> value, bool warning)
            => Assign(WidgetUtility.CopyToClipboard(label, value, null, warning, Parent));

        public DebugTextPair CopyToClipboard(string label, string value) => Assign(WidgetUtility.CopyToClipboard(label, value, Parent));
            
        public DebugTextPair CopyToClipboard(string label, Func<string> refreshFunc) => Assign(WidgetUtility.CopyToClipboard(label, refreshFunc, Parent));

        public DebugTextPair CopyToClipboard(string label, string value, Func<string> valueToCopy)
            => Assign(WidgetUtility.CopyToClipboard(label, value, valueToCopy, Parent));

        public DebugTextPair CopyToClipboard(string label, Func<string> value, Func<string> valueToCopy, bool warning)
            => Assign(WidgetUtility.CopyToClipboard(label, value, valueToCopy, warning, Parent));

        public DebugButtonWithInputField Button(string label, Action callback)
            => Assign(WidgetUtility.Button(label, callback, Parent));

        public DebugButtonWithInputField ButtonWithInput(string label, string placeholderText, Action<string> callback, Func<string> refreshFunc = null)
            => Assign(WidgetUtility.ButtonWithInput(label, placeholderText, callback, refreshFunc, Parent));
        
        public DebugButtonWithInputField InputField(string label, string placeholderText, Action<string> callback, Func<string> refreshFunc = null)
            => Assign(WidgetUtility.InputField(label, placeholderText, callback, refreshFunc, Parent));

        public DebugToggleButton Toggle(string label, bool value, Action<bool> callback)
            => Assign(WidgetUtility.Toggle(label, value, callback, Parent));

        public DebugRowLabel RowLabel(string rowName, string value)
            => Assign(WidgetUtility.RowLabel(rowName, value, Parent));
        
        public DebugRowLabel RowLabel(string rowName, string[] values)
            => Assign(WidgetUtility.RowLabel(rowName, values, Parent));
        
        public DebugCheckboxButton CheckboxButton(string label, Action<bool> callback)
            => Assign(WidgetUtility.CheckboxButton(label, callback, Parent));

        public DebugRadioButton RadioButton(string label, Action<bool> callback)
        {
            if (_currentRadioGroup == null) {
                throw new Exception("Trying to create a new radio button without a group. Call 'CreateRadioGroup' before");
            }

            DebugRadioButton radioBtn = WidgetUtility.RadioButton(label, callback, Parent);
            Assign(radioBtn);
            _currentRadioGroup.Add(radioBtn);
            return radioBtn;
        }

        public void ResetRadioGroups(bool callCallback)
        {
            foreach (DebugRadioGroup group in _radioGroups)
            {
                group.ResetToggles(callCallback);
            }
        }

        public int CompareTo(Screen other) => orderIndex.CompareTo(other.orderIndex);

        public int CompareTo(object obj) => CompareTo((Screen)obj);
    }

    public interface IConditionalScreen
    {
        public bool CanDisplay { get; }
    }
}
