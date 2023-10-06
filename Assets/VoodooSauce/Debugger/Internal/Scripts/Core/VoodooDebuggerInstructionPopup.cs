using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Internal.Extension;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.GDPR.UI
{
	public class VoodooDebuggerInstructionPopup : MonoBehaviour
	{
		const string _missingConsent = "You need to consent in order to have access to the debugger.";
		const string _deviceIDPrefix = "Idfv :";
		const string _secondPairPrefix = "Idfa :";

		[SerializeField] private GameObject _idParent;
		[SerializeField] private Text _deviceId;
		[SerializeField] private Button _copyButton;
		[SerializeField] private GameObject _secondPair;
		[SerializeField] private Text _secondPairText;
		[SerializeField] private Button _secondPairbutton;
		[SerializeField] private DebugPopup _debugPopup;

		private string _deviceIdValue;
		private string _secondPairValue;

		private void Start()
		{
			_copyButton.onClick.AddListener(() => _deviceIdValue.CopyToClipboard());
			_secondPairbutton.onClick.AddListener(() => _secondPairValue.CopyToClipboard());
		}

		private void OnEnable()
		{
			_debugPopup.Initialize(() => gameObject.SetActive(false));
			transform.RefreshHierarchySize();
		}

		private void OnDisable()
		{
			_debugPopup.Dispose();
		}

		public void Show(string instruction, bool showUserId, UnityAction buttonCallback = null)
		{
			var privacy = VoodooSauceCore.GetPrivacy();

			DisplayInstructions(showUserId, privacy.GetVendorId());
			DisplaySecondInstructions(showUserId && privacy.HasAdsConsent(), privacy.GetAdvertisingId(false));
            
			gameObject.SetActive(true);
			_debugPopup.Initialize(instruction, buttonCallback);
		}

		private void DisplayInstructions(bool showUserId, string deviceId = null)
		{
			_idParent.SetActive(showUserId);
			_deviceIdValue = deviceId;
			_deviceId.text = _deviceIDPrefix + _deviceIdValue;
		}

		private void DisplaySecondInstructions(bool showUserId, string deviceId = null)
		{
			_secondPair.SetActive(showUserId);
			_secondPairValue = deviceId;
			_secondPairText.text = _secondPairPrefix + _secondPairValue;
		}
	}
}