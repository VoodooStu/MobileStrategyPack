using System;
using System.Collections.Generic;
using Voodoo.Sauce.Internal;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Common.Utils
{
    public class HandledAction<T>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();
        private readonly string _tag;
        
        public HandledAction(string tag) => _tag = tag;

        public void Invoke(T t)
        {
            foreach (Action<T> action in _actions.ToArray()) {
                try {
                    action.Invoke(t);
                } catch (Exception e) {
                    VoodooLog.LogWarning(Module.COMMON, _tag, $"Exception caught: {e}");
                }
            }
        }

        public void Add(Action<T> action)
        {
            if (_actions.Contains(action)) {
                return;
            }
            
            _actions.Add(action);
        }

        public void Remove(Action<T> action)
        {
            if (!_actions.Contains(action)) {
                return;
            }

            _actions.Remove(action);
        }

        public void Clear()
        {
            _actions.RemoveAll(a => true);
        }
    }
}