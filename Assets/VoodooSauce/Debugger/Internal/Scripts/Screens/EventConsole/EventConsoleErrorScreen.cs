using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Debugger
{
    public class EventConsoleErrorScreen : Screen
    {
        [SerializeField]
        private Button errorCopyButton;
        [SerializeField]
        private Text errorMessage;

        public void ShowErrorMessage(DebugAnalyticsLog log)
        {
            Debugger.Show(this);
            errorMessage.text = log.Error;
            errorCopyButton.onClick.RemoveAllListeners();
            errorCopyButton.onClick.AddListener(() => errorMessage.text.CopyToClipboard());
            errorMessage.transform.RefreshHierarchySize();
        }
    }
}