using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

#if UNITY_IOS && !UNITY_EDITOR
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
#endif

namespace Voodoo.Sauce.Internal.CrossPromo.Canvas
{
    internal class SquareFormat : AFormatManager
    {
        private VideoManager _videoManager;
        private int _retries;

        private void Awake()
        {
            Instance = this;
            _retries = 1;
            gameObject.name = "CrossPromoGameObject";
            CrossPromoDisplayEvents.ShowEvent += Display;
            CrossPromoDisplayEvents.HideEvent += Hide;
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
                _videoManager.VideoPlayer.loopPointReached += source => {
                    _videoManager.Content.enabled = false;
                    Display();
                };
                _videoManager.VideoPlayer.prepareCompleted += source => {
                    _videoManager.Content.enabled = true;
                    CrossPromoEvents.TriggerShownEvent(Asset);
                };
            } catch (Exception e) {
                VoodooLog.LogError(Module.CROSS_PROMO, VoodooCrossPromo.TAG, e.ToString());
            }
        }

        public override void Display()
        {
            Asset = null;
            IsActive = true;
            if (!IsWaiting)
                StartCoroutine(StartDisplay());
        }

        private IEnumerator StartDisplay()
        {
            IsWaiting = true;
            yield return new WaitForEndOfFrame();
            yield return WaitCrossPromoReady();
            IsWaiting = false;
            if (!IsActive || Asset == null) {
                if (!IsActive)
                    Hide();
                else
                    _videoManager.StopVideo();
                StartCoroutine(Retry());
                yield break;
            }

            _retries = 1;
            try {
                _videoManager.PrepareVideo(Asset);
            } catch (Exception e) {
                CrossPromoEvents.TriggerErrorEvent(e);
            }
        }

        private void OnDisable()
        {
            IsWaiting = false;
            Hide();
        }

        private void OnEnable()
        {
            IsWaiting = false;
        }

        public override void Hide()
        {
            IsActive = false;
            IsWaiting = false;
            _retries = 1;
            _videoManager?.StopVideo();
        }

        private IEnumerator Retry()
        {
            if (_retries > 10)
                yield break;
            var waitInSeconds = (int) Math.Pow(2, _retries);
            _retries += 1;
            yield return new WaitForSeconds(waitInSeconds);
            while (!IsActive) yield return null;
            Display();
        }

#if UNITY_IOS && !UNITY_EDITOR
        public override void OnClickEvent()
        {
            _videoManager.VideoPlayer.Pause();
            base.OnClickEvent();            
        }

        public void AppstoreClosed(string message)
        {
            _videoManager.VideoPlayer.Play();
            if (message.Contains("error"))
            {
                if (Asset?.game != null)
                    IOSCrossPromoWrapper.LoadNativeStore(Asset.game.apple_id);
            }
        }
#endif
    }
}