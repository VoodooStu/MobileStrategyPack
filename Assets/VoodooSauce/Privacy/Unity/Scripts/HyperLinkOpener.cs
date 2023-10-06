using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HyperLinkOpener : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(GetComponent<TextMeshProUGUI>(), Input.mousePosition, null);
        if( linkIndex != -1 ) {
            TMP_LinkInfo linkInfo = GetComponent<TextMeshProUGUI>().textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}