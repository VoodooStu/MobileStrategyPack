using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class DeleteScreen : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _intro;
        [SerializeField] private Text _emailTitle;
        [SerializeField] private InputField _email;
        [SerializeField] private Text _idfaTitle;
        [SerializeField] private InputField _idfa;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Text _deleteText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _closeText;

        private UnityAction<string> _deleteCallback;
        
        public class Parameters
        {
            public Color mainColor;

            public string title; //delete_data
            public string intro; //intro_delete
            public string emailTitle; //email
            public string idfaTitle;
            public string idfa;
            public string deleteText;//confirm
             public UnityAction<string> deleteCallback;
            public string closeText;//close
            public UnityAction closeCallback;
        }

        public void Initialize(Parameters p)
        {
            _title.text = p.title;
            _intro.text = p.intro;
            _emailTitle.text = p.emailTitle;
            _idfaTitle.text = p.idfaTitle;
            _idfa.text = p.idfa;
            _deleteButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.AddListener(RequestDelete);
            _deleteButton.GetComponent<Image>().color = p.mainColor;
            _deleteCallback = p.deleteCallback;
            _deleteText.text = p.deleteText;
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(p.closeCallback);
            _closeText.text = p.closeText;
            _closeText.color = p.mainColor;
        }

        private void RequestDelete()
        {
            _deleteCallback.Invoke(_email.text);
        }
    }
}
