using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Tools.AccessButton
{
    public class HidePanel : MonoBehaviour
    {
        public delegate void OnHideChangeState(bool appear);

        public static OnHideChangeState HideChangeState;

        public CanvasGroup canvasGroup;
        public Image background;
        public RectTransform logo;

        private bool _visible;

        // Logo
        private const float LogoOnY = 0f;
        private const float LogoOffY = -256f;

        private const float LogoSpeedAppear = 20f;

        // Background
        private const float BackgroundOn = 0.39f;
        private const float BackgroundOff = 0f;

        private const float BackgroundSpeedAppear = 0.02f;

        //Async
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private void Start()
        {
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
            logo.localPosition = new Vector3(0, LogoOffY, 0);

            HideChangeState += ChangeState;

            GroupVisibility(false);
        }

        private void ChangeState(bool appear)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            UIDisappear();

            if (appear)
            {
                if (_visible == false)
                {
                    UIAppear();
                }
            }
            else
            {
                if (_cancellationTokenSource != null && _cancellationTokenSource.Token.CanBeCanceled)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
        }

        private async void UIAppear()
        {
            GroupVisibility(true);

            while ((background.color.a < BackgroundOn || logo.localPosition.y < LogoOnY)
                   && _cancellationTokenSource.IsCancellationRequested == false)
            {
                if (background.color.a < BackgroundOn)
                {
                    float alpha = background.color.a;
                    background.color = new Color(background.color.r, background.color.g, background.color.b,
                        alpha += BackgroundSpeedAppear);
                }

                if (logo.localPosition.y < LogoOnY)
                {
                    logo.localPosition += new Vector3(0, LogoSpeedAppear, 0);
                }

                await Task.Delay(25);
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                UIDisappear();
                _cancellationTokenSource = new CancellationTokenSource();
                return;
            }

            _visible = true;
        }

        private void UIDisappear()
        {
            GroupVisibility(false);

            background.color = new Color(background.color.r, background.color.g, background.color.b,
                BackgroundOff);

            Vector3 position = logo.localPosition;

            logo.localPosition = new Vector3(position.x, LogoOffY, position.z);

            _visible = false;
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Dispose();
            HideChangeState -= ChangeState;
        }

        private void GroupVisibility(bool show)
        {
            canvasGroup.interactable = show;
            canvasGroup.alpha = show ? 1f : 0f;
        }
    }
}