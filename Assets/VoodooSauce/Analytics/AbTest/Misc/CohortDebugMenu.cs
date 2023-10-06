using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Core;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.Misc
{
    public class CohortDebugMenu : MonoBehaviour
    {
        private const string TAG = "CohortDebugMenu";
        [SerializeField]
        private Transform _content;

        private List<Button> _buttons;
        private const string ClosingButtonMagicString = "123456789";

        private void Start()
        {
            if (_buttons == null) {
                _buttons = new List<Button>();
                _buttons.Add(CreateButton(null));

                string[] abTests = VoodooSauce.GetAbTests();
                
                if (abTests != null)
                {
                    foreach (string cohort in abTests)
                    {
                        _buttons.Add(CreateButton(cohort));
                    }
                }

                _buttons.Add(CreateButton(ClosingButtonMagicString));
            }
        }

        private Button CreateButton(string cohort)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<Image>();
            Button btn = obj.AddComponent<Button>();

            GameObject child = new GameObject();
            child.transform.SetParent(obj.transform, false);
            Text txt = child.AddComponent<Text>();

            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            txt.rectTransform.anchorMin = new Vector2(0, 0);
            txt.rectTransform.anchorMax = new Vector2(1, 1);
            txt.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            txt.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            if (cohort == null) {
                txt.text = "None";
            } else if (cohort == ClosingButtonMagicString) {
                txt.text = "Close Menu";
            } else {
                txt.text = cohort;
            }

            if (cohort == ClosingButtonMagicString) {
                btn.onClick.AddListener(VoodooSauceBehaviour.CloseCohortDebugMenu);
            } else {
                btn.onClick.AddListener(() => {
                    VoodooLog.LogDebug(Module.ANALYTICS, TAG, "Setting player cohort to: " + (cohort ?? "None"));
                    VoodooSauce.SetPlayerCohort(cohort);
                });
            }

            obj.transform.SetParent(_content, false);

            return btn;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}