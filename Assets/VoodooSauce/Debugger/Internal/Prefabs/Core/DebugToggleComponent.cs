using UnityEngine;
using Voodoo.Sauce.Debugger;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public abstract class DebugToggleComponent : MonoBehaviour
    {
        
#region UI components
        
        protected DebugToggleButton Button {
            get {
                if (_button != null) {
                    return _button;
                }

                _button = GetComponent<DebugToggleButton>();
                return _button;
            }
        }
        private DebugToggleButton _button;
        
#endregion
        
#region MonoBehaviour's life cycle

        protected virtual void Start()
        {
            Button.SetValue(DefaultButtonState());
        }
        
#endregion
        
#region Life cycle

        protected abstract bool DefaultButtonState();
        
        public virtual void SetEnabled(bool isEnabled)
        {
            Button.SetValue(isEnabled);
        }
        
        protected void UpdateEnabled()
        {
            Button.SetValue(DefaultButtonState());
        }

#endregion

    }
}