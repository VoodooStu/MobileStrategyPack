using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class PopupScreen : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _message;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _closeButtonText;

        private Action _closeCallback;
        
        public class Parameters
        {
            public Color mainColor;

            public string title;
            public string message;
            public UnityAction closeCallback;
            public string closeButtonText;
        }

        public void Initialize(Parameters p)
        {
            _title.text = p.title;
            _message.text = p.message;
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(p.closeCallback);
            _closeButton.GetComponent<Image>().color = p.mainColor;
            _closeButtonText.text = p.closeButtonText;
        }
    }
}