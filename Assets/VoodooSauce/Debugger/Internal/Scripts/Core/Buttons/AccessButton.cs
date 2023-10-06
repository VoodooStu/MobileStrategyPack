using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Analytics;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Tools.AccessButton
{
    public class AccessButton : MonoBehaviour
    {
        [SerializeField] private GameObject voodooSauceLogo;
        [SerializeField] private GameObject recordingLogo;
        [SerializeField] private BubbleButton bubbleButton;
        [SerializeField] private HidePanel hidePanel;

        private void Start()
        {
            AccessProcess.ButtonInstance = gameObject;

            bubbleButton.gameObject.SetActive(true);
            hidePanel.gameObject.SetActive(true);

            SetRecordingAnimationEnabled(AnalyticsEventLogger.GetInstance().IsRecordingEvents);
            AnalyticsEventLogger.GetInstance().OnRecordingStateChange += SetRecordingAnimationEnabled;
            
            DontDestroyOnLoad(gameObject);
        }

        private void SetRecordingAnimationEnabled(bool isRecording)
        {
            voodooSauceLogo.SetActive(!isRecording);
            recordingLogo.SetActive(isRecording);
            if (isRecording)
            {
                StartCoroutine(RecordingLogoCoroutine());
            }
            else
            {
                StopAllCoroutines();
            }
        }

        private void OnDestroy()
        {
            AnalyticsEventLogger.GetInstance().OnRecordingStateChange -= SetRecordingAnimationEnabled;
        }

        private IEnumerator RecordingLogoCoroutine()
        {
            var recordingImage = recordingLogo.GetComponent<Image>();
            var color = recordingImage.color;
            color.a = 1f;
            recordingImage.color = color;
            while (true)
            {
                yield return new WaitForSeconds(.9f);
                // Fade out
                while (color.a > 0f)
                {
                    color.a -= Time.deltaTime * 1.75f;
                    recordingImage.color = color;
                    yield return null;
                }
                // Fade in
                while (color.a < 1f)
                {
                    color.a += Time.deltaTime * 1.75f;
                    recordingImage.color = color;
                    yield return null;
                }
            }
        }
        
    }
}