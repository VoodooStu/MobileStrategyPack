using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Extension;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class EventInformationList : MonoBehaviour
    {
        [SerializeField]
        private Text title;
        [SerializeField]
        private Transform container;

        public void Initialize(string header)
        {
            title.text = header.BoldText();
        }

        public Transform GetContainer => container;
    }
}