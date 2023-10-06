using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Voodoo.Sauce.Debugger;

public class AnalyticsProviderButton : Widget
{
    [SerializeField]
    private Button testButton;
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text descText;

    public void SetTitleText(string text)
    {
        titleText.text = text;
    }

    public void SetDescText(string text)
    {
        descText.text = text;
    }

    public void SetOnClickListener(UnityAction onClickAction)
    {
        testButton.onClick.AddListener(onClickAction);
    }

    public void SetButtonColor(Color color)
    {
        testButton.image.color = color;
    }
}
