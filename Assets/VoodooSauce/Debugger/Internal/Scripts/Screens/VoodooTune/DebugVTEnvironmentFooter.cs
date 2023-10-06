using UnityEngine;
using UnityEngine.UI;
using Voodoo.Tune.Core;

// ReSharper disable HeuristicUnreachableCode

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    public class DebugVTEnvironmentFooter : MonoBehaviour
    {
        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            _text.text = $"{VoodooTunePersistentData.SavedServer} | {VoodooTunePersistentData.SavedStatusName}";
        }
    }
}