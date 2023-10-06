using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

#if UNITY_IOS && !UNITY_EDITOR
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
#endif

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.CrossPromo.Canvas
{
    /// <inheritdoc />
    /// <summary>
    /// Default Format
    /// </summary>
    internal class DefaultFormat : AFormatManager
    {
        private Text _gameName;

        private VideoManager _videoManager;
        [SerializeField]
        private GameObject _adLabel;
        [SerializeField]
        public RectTransform myCanvasTransformer;

        // please add the crossPromo transform 
        private static RectTransform _transformer;

        private void Awake()
        {
            Instance = this;
            gameObject.name = "CrossPromoGameObject";
            CrossPromoDisplayEvents.ShowEvent += Display;
            CrossPromoDisplayEvents.HideEvent += Hide;
            _transformer = myCanvasTransformer;
        }

        private void OnDestroy()
        {
            CrossPromoDisplayEvents.ShowEvent -= Display;
            CrossPromoDisplayEvents.HideEvent -= Hide;
        }

        private void Start()
        {
            try {
                _videoManager = new VideoManager(
                    GetComponentInChildren<VideoPlayer>(),
                    GetComponentInChildren<RawImage>()
                );
                _gameName = GetComponentInChildren<Text>();
                _videoManager.VideoPlayer.prepareCompleted += source => {
                    if (Asset == null) {
                        Hide();
                    } else {
                        Enable(true);
                        CrossPromoEvents.TriggerShownEvent(Asset);
                    }
                };
                Enable(false);
            } catch (Exception e) {
                VoodooLog.LogError(Module.CROSS_PROMO, VoodooCrossPromo.TAG, e.ToString());
            }
        }

        public override void Display()
        {
            if (IsWaiting)
                IsActive = true;
            if (!IsActive && !IsWaiting)
                StartCoroutine(StartDisplay());
        }

        private IEnumerator StartDisplay()
        {
            IsActive = true;
            IsWaiting = true;
            yield return new WaitForEndOfFrame();
            yield return WaitCrossPromoReady();
            IsWaiting = false;
            if (!IsActive || Asset == null)
                yield break;

            if (Asset.game.name != null)
                _gameName.text = Asset.game.name;
            try {
                _videoManager.PrepareVideo(Asset);
            } catch (Exception e) {
                CrossPromoEvents.TriggerErrorEvent(e);
            }
        }

        // retrieve screen size for voodoo video
        public static Vector3 RectTransformToScreenSpace()
        {
            var worldCorners = new Vector3[4];
            _transformer.GetWorldCorners(worldCorners);

            Vector3 bottomLeft = worldCorners[0];
            bottomLeft.y = Screen.height - bottomLeft.y;
            return bottomLeft;
        }

        public override void Hide()
        {
            IsActive = false;
            Asset = null;
            if (_videoManager != null) {
                _videoManager.StopVideo();
                Enable(false);
            }
        }

        private void Enable(bool show)
        {
            GetComponent<Image>().enabled = show;
            transform.GetChild(0).gameObject.SetActive(show);
            transform.GetChild(1).GetComponentInChildren<Image>().enabled = show;
#if UNITY_ANDROID
            _adLabel.SetActive(show);
#else
            _adLabel.SetActive(false);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        public void AppstoreClosed(string message)
        {
            if (message.Contains("error"))
            {
                if (Asset?.game != null)
                    IOSCrossPromoWrapper.LoadNativeStore(Asset.game.apple_id);
            }
        }
#endif
    }
}