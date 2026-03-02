using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIUtils
{
    public enum EButtonAction
    {
        Click,
        MouseDown,
        MouseUp,
        MouseExit
    }
	
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ButtonListener : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] private bool _playAnimation = false;
        
        public static bool AllButtonsEnabled => _locked.Count == 0;
        private static readonly HashSet<string> _locked = new();
        public Action onClick { get; set; }
        public Action onDown { get; set; }
        public Action onUp { get; set; }
        public Action onExit { get; set; }
        public Action<bool> onPress { get; set; }
        public Action onEnter { get; set; }
        
        public static event Action<bool> OnButtonsEnableChanged;
        
        private bool _buttonHolded;
        private float _buttonDownTime;
        
        private TweenerCore<Vector3, Vector3, VectorOptions> _animationTweenerCore;
        private Vector3 _defaultScale;
        
        private const float ANIMATION_TIME = 0.1f;
        private const float INCREASE_MULTIPLIER = 0.9f;
        
        public void SetFunctionToButtonDownAndHold(Action callBackDown, Action callBackHold, int holdDurationMS = 500)
        {
            this.onUp = null;
            this.onClick = null;
            this.onDown = null;
            this.onExit = null;

            ButtonListener.OnButtonsEnableChanged += (bool value) =>
            {
                if (!value)
                    _buttonHolded = false;
            };
            
            float holdDurationSec = (float) holdDurationMS / 1000f;
            
            this.onUp += () =>
            {
                _buttonHolded = false;
            };
            
            this.onDown += () =>
            {
                _buttonDownTime = Time.time;
                _buttonHolded = true;
                RiseEventOnButton(callBackDown);
                HoldTimer(callBackHold, holdDurationSec).Forget();
            };
        }
        
        private bool ClickNotAvailable(PointerEventData eventData)
        {
            return !AllButtonsEnabled ||
                   !IsActive();
        }

        private static void RiseEventOnButton(Action callBack)
        {
            callBack?.Invoke();
        }

        private async UniTask HoldTimer(Action callBackHold, float holdDurationSec)
        {
            while (_buttonHolded)
            {
                await UniTask.DelayFrame(1);

                if (Time.time - _buttonDownTime > holdDurationSec)
                {
                    callBackHold?.Invoke();
                    return;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (ClickNotAvailable(eventData))
                return;

            if (_playAnimation)
            {
                PlayDownAnimation();
            }

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            onDown?.Invoke();
            onPress?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_playAnimation)
            {
                PlayUpAnimation();
            }
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (ClickNotAvailable(eventData))
                return;

            onUp?.Invoke();
            onPress?.Invoke(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (ClickNotAvailable(eventData))
                return;

            onExit?.Invoke();
            onPress?.Invoke(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (ClickNotAvailable(eventData))
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (ClickNotAvailable(eventData))
                return;

            onEnter?.Invoke();
        }
        
        private void PlayDownAnimation()
        {
            StopPreviousAnimation();

            if (_defaultScale == Vector3.zero)
                _defaultScale = transform.localScale;

            _animationTweenerCore = transform.DOScale(_defaultScale * INCREASE_MULTIPLIER, ANIMATION_TIME);
        }

        private void PlayUpAnimation()
        {
            StopPreviousAnimation();

            _defaultScale = transform.localScale;
            _animationTweenerCore = transform.DOScale(_defaultScale, ANIMATION_TIME);
        }
        
        private void StopPreviousAnimation()
        {
            _animationTweenerCore?.Kill();
            
            if (_defaultScale != Vector3.zero)
                transform.localScale = _defaultScale;
        }
    }
}