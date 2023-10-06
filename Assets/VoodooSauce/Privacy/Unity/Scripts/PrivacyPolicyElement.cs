using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Text))]
    [RequireComponent(typeof(Button))]
    public class PrivacyPolicyElement : MonoBehaviour
    {
        public struct ConsentLineParameters
        {
            public string text;
            public UnityAction buttonAction;
        }

        public void Initialize(ConsentLineParameters p)
        {
            GetComponent<Text>().text = p.text;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(p.buttonAction);
        }
    }
}
