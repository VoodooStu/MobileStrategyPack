using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.GDPR.UI;

namespace Voodoo.Sauce.Tools.AccessButton
{
    public class BubbleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler
    {
        // For Testing only, simulate the upside down in editor
        public bool forceUpsideDown;
        public CanvasGroup canvasGroup;

        // Component
        public RectTransform parentRT;
        public RectTransform myRect;
        public RectTransform hideArea;

        // UI Visibility
        private bool _isUsable = true;
        private const float MAXSize = 1.5f;
        private const float DistanceHide = -1200f;

        // State
        private bool _isDragged;

        // Drag behavior
        private Vector3 _startMousePos;
        private Vector3 _startPos;

        // Keep inside screen
        private bool _isXRestrict;
        private bool _isYRestrict;
        private float _fakeX;
        private float _fakeY;
        private float _halfWidth;
        private float _halfHeight;

        // Lerp
        private Vector3 _endLerpPosition;
        private Vector3 _startLerpPosition;
        private const float DurationLerp = 0.1f;

        // Hide Functions
        private EventSystem _eventSystem;

        //Async
        private static CancellationTokenSource _mouseDownCts;
        
        private const string TAG = "AccessButton";

        private void Start()
        {
            _eventSystem = EventSystem.current;

            if (_eventSystem == null)
                VoodooLog.LogDebug(Module.COMMON, TAG, "No EventSystem on the scene. Add an EventSystem to use the Access Button");

            Rect rect = myRect.rect;
            _halfWidth = rect.width / 2f;
            _halfHeight = rect.height / 2f;

            GroupVisibility(true);
        }
        
        private void Update()
        {
            if (_isUsable == false)
            {
                if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || forceUpsideDown)
                {
                    forceUpsideDown = false;
                    ShowButtonUI();
                }
            }
        }

        private void SetUpLerp()
        {
            // setup Lerp
            _startLerpPosition = myRect.localPosition;
            if (myRect.localPosition.x <= 0)
            {
                _endLerpPosition = new Vector3(parentRT.rect.xMin + _halfWidth, _startLerpPosition.y,
                    _startLerpPosition.z);
            }
            else if (myRect.localPosition.x > 0)
            {
                _endLerpPosition = new Vector3(parentRT.rect.xMax - _halfWidth, _startLerpPosition.y,
                    _startLerpPosition.z);
            }
        }

        private async void HideButtonUI()
        {
            _isUsable = false;

            while (myRect.transform.localPosition.y > DistanceHide)
            {
                myRect.transform.localPosition -= new Vector3(0, 100, 0);
                await Task.Delay(1);
            }

            myRect.transform.localPosition = new Vector3(0, DistanceHide, 0);
            GroupVisibility(false);
        }

        private async void ShowButtonUI()
        {
            GroupVisibility(true);
            myRect.transform.localPosition = new Vector3(0, -200, 0);

            SetUpLerp();

            await Task.Delay(500);
            EndOfDragBehavior();
            await Task.Delay(500);

            _isUsable = true;
        }

        private async void EndOfDragBehavior()
        {
            float time = 0;
            _mouseDownCts = new CancellationTokenSource();
            while (_mouseDownCts.IsCancellationRequested == false)
            {
                Resize();

                if (time < DurationLerp)
                {
                    transform.localPosition = Vector3.Lerp(_startLerpPosition, _endLerpPosition, time / DurationLerp);
                    time += Time.deltaTime;
                }
                else
                {
                    transform.localPosition = _endLerpPosition;
                    _mouseDownCts.Cancel();
                }

                await Task.Delay(1);
            }

            KeepInsideScreen();
        }

        private void Resize()
        {
            // Resize
            if (transform.localScale.x > 1)
                transform.localScale -= new Vector3(0.2f, 0.2f, 0.2f);
            else
                transform.localScale = new Vector3(1, 1, 1);
        }

        private void KeepInsideScreen()
        {
            if (myRect.localPosition.x < parentRT.rect.xMin + _halfWidth - 1 ||
                myRect.localPosition.x > parentRT.rect.xMax - _halfWidth + 1)
                _isXRestrict = true;
            else
                _isXRestrict = false;

            if (myRect.localPosition.y < parentRT.rect.yMin + _halfHeight ||
                myRect.localPosition.y > parentRT.rect.yMax - _halfHeight)
                _isYRestrict = true;
            else
                _isYRestrict = false;

            if (_isXRestrict)
            {
                if (myRect.localPosition.x < 0)
                    _fakeX = parentRT.rect.xMin + _halfWidth;
                else
                    _fakeX = parentRT.rect.xMax - _halfWidth;

                myRect.localPosition = new Vector3(_fakeX, myRect.localPosition.y, 0.0f);
            }

            if (_isYRestrict)
            {
                if (transform.localPosition.y < 0)
                    _fakeY = parentRT.rect.yMin + _halfHeight;
                else
                    _fakeY = parentRT.rect.yMax - _halfHeight;

                myRect.localPosition = new Vector3(transform.localPosition.x, _fakeY, 0.0f);
            }
        }

        private void GroupVisibility(bool show)
        {
            canvasGroup.interactable = show;

            canvasGroup.alpha = show ? 1f : 0f;
        }

        private void OnClick()
        {
            VoodooSauceBehaviour.OpenDebugger();
        }

        // Handlers

        public void OnPointerDown(PointerEventData ped)
        {
            _startPos = transform.position;
            _startMousePos = Input.mousePosition;

            HidePanel.HideChangeState?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData ped)
        {
            if (_isDragged == false) OnClick();

            SetUpLerp();

            // Check for hiding the button
            if (RectTransformUtility.RectangleContainsScreenPoint(hideArea, Input.mousePosition)) HideButtonUI();

            HidePanel.HideChangeState?.Invoke(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_mouseDownCts != null && _mouseDownCts.Token.CanBeCanceled) _mouseDownCts.Cancel();

            _isDragged = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Size Up
            if (transform.localScale.x < MAXSize)
                transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            else
                transform.localScale = new Vector3(MAXSize, MAXSize, MAXSize);

            Vector3 currentPos = Input.mousePosition;
            Vector3 diff = currentPos - _startMousePos;
            Vector3 pos = _startPos + diff;
            transform.position = pos;

            KeepInsideScreen();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragged = false;
            EndOfDragBehavior();
        }
    }
}