using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class CohortButton : Widget
    {
        [SerializeField]
        private Button button;
    
        [SerializeField]
        private Text titleText;

        public void SetTitleText(string text)
        {
            titleText.text = text;
        }

        public void SetOnClickListener(UnityAction onClickAction)
        {
            button.onClick.AddListener(onClickAction);
        }
    }
}