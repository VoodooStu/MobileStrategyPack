using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Voodoo.Sauce.Debugger
{
    public class DebugHideableSection : Widget
    {
        [SerializeField] private Button buttonShowHide;
        [SerializeField] private Image showHideImage;
        [SerializeField] private Sprite collapseSprite;
        [SerializeField] private Sprite expandSprite;

        private readonly List<GameObject> children = new List<GameObject>();

        private bool isShown = true;

        private void Awake()
        {
            showHideImage.sprite = collapseSprite;

            buttonShowHide.onClick.AddListener(() => {
                isShown = !isShown;
                SetChildrenVisibility(isShown);

                showHideImage.sprite = (isShown ? collapseSprite : expandSprite);
            });
        }

        public void AddChild(GameObject child)
        {
            children.Add(child);
        }

        private void SetChildrenVisibility(bool show)
        {
            foreach (GameObject child in children) {
                child.SetActive(show);
            }
        }
    }
}