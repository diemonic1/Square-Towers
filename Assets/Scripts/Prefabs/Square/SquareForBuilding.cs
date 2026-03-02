using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Prefabs.Fields;
using Prefabs.SquareContainers;
using Services.ConfigsService;
using Services.LocalizationService;
using Services.Monobehs;
using Services.SavingService.SavingModels;
using UIUtils;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Prefabs
{
    public class SquareForBuilding : Square
    {
        [SerializeField] private float _rotatingOnDestroyDuration;
        [SerializeField] private float _destroyAnimationDuration;
        [SerializeField] private float _canAddAnimationDuration;

        [SerializeField] private BoxCollider2D _boxCollider2D;

        [SerializeField] private int _hideMessageAfterMS;
        
        [Header("Move Animation")]
        [SerializeField] private float _moveDuration = 0.4f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        
        [Header("Jump Animation")]
        [SerializeField] private float _jumpDuration = 0.4f;
        [SerializeField] private float _jumpHeight = 100f;
        [SerializeField] private float _rotationDuarationRelativeToEntireJump = 0.9f;
        [SerializeField] private int _fullRotationsOnJump = 2;
        [SerializeField] private Ease _jumpEase = Ease.Linear;
        
        public bool AnimatingNow { get; private set; }
        public bool FallAnimationNow { get; private set; }
        public Vector2 GetSquareSize => new Vector2(_boxCollider2D.size.x, _boxCollider2D.size.y);

        public Tower Tower { get; private set; } = null;
        
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTweener;

        private bool _thisSquareMovingNow;
        
        private List<SquareForBuilding> _otherSquaresNearby = new List<SquareForBuilding>();

        private RightField _rightFieldContainer;
        private BlackHole _blackHoleContainer;

        private Vector2 _lastSavedPositionInTower;

        private EAddingSquareResult _lastAddingSquareResult = EAddingSquareResult.fail_squareInEmptyPlace;

        private Vector2 _currentLocalPositionTarget;
        
        [Inject] private ConfigsService _configsService;
        [Inject] private LocalizationService _localizationService;
        
        public void SetTower(Tower tower)
        {
            Tower = tower;
            
            _button.SetFunctionToButtonDownAndHold(
                () =>
                {
                    if (!Tower.IsAnySquareAnimatingNow)
                    {
                        _animationHandler
                            .DOScale(_scaleToOnSelect, (float)_configsService.GameConfig.HoldingTimeToGetSquareFromScrollInMs / 1000)
                            .OnComplete(() =>
                            {
                                _animationHandler.DOScale(Vector3.one, _durationOfScaleRevert);
                            });
                    }
                },
                () =>
                {
                    if (!Tower.IsAnySquareAnimatingNow)
                    {
                        _lastSavedPositionInTower = transform.localPosition;
                        Tower.SpawnGhostSquare(_lastSavedPositionInTower);
                        SquareMoveService.Instance.AddExistingSquareInHand(this);
                    }
                }, 
                _configsService.GameConfig.HoldingTimeToGetSquareFromScrollInMs);
        }

        public SquareForSave GetSquareForSave()
        {
            return new SquareForSave(_currentLocalPositionTarget, _color);
        }
        
        public void TryAddToLastSquareContainer()
        {
            Tower possibleTowerToAdd = null;
            EAddingSquareResult addingSquareToTowerResult = _lastAddingSquareResult;
            
            foreach (var otherSquare in _otherSquaresNearby)
            {
                addingSquareToTowerResult = otherSquare.Tower.GetCanAddSquareStatus(this);

                if (otherSquare.Tower != null 
                    && otherSquare.Tower != Tower
                    && addingSquareToTowerResult == EAddingSquareResult.success)
                {
                    possibleTowerToAdd = otherSquare.Tower;
                }
            }

            if (_blackHoleContainer != null)
            {
                addingSquareToTowerResult = _blackHoleContainer.GetCanAddSquareStatus(this);

                if (addingSquareToTowerResult == EAddingSquareResult.success)
                {
                    InfoMessageService.Instance.ShowMessage(
                        _localizationService.Localize("#SquareDeleted"), _hideMessageAfterMS);

                    _blackHoleContainer.AddSquare(this);
                }
                return;
            }
            
            if (possibleTowerToAdd != null)
            {
                InfoMessageService.Instance.ShowMessage(
                    _localizationService.Localize("#SquareAdded"), _hideMessageAfterMS);
                
                possibleTowerToAdd.AddSquare(this);
                return;
            }
            
            if (_rightFieldContainer != null)
            {
                addingSquareToTowerResult = _rightFieldContainer.GetCanAddSquareStatus(this);

                if (addingSquareToTowerResult == EAddingSquareResult.success)
                {
                    InfoMessageService.Instance.ShowMessage(
                        _localizationService.Localize("#TowerAdded"), _hideMessageAfterMS);
                
                    _rightFieldContainer.AddSquare(this);
                    return;
                }
            }
            
            if (Tower != null)
            {
                transform.SetParent(Tower.transform);
                MoveToPoint(_lastSavedPositionInTower, 
                    Tower.DestroyGhostSquare);
                
                return;
            }
            
            InfoMessageService.Instance.ShowMessage(
                _localizationService.Localize("#SquareDisappear_" + addingSquareToTowerResult), _hideMessageAfterMS);
                
            DestroySquare(true);
        }
        
        public void InitMovingSubscribe(ReactiveProperty<bool> movingSquareNow)
        {
            movingSquareNow.Subscribe(value =>
            {
                _thisSquareMovingNow = value;
            }).AddTo(this);
        }

        public void AnimateJump(Vector3 localTarget)
        {
            AnimatingNow = true;
            _currentLocalPositionTarget = localTarget;
            
            Vector3 startPos = transform.localPosition;

            float distance = Vector3.Distance(startPos, localTarget);
            float duration = _jumpDuration * Mathf.Clamp(distance / 5f, 0.5f, 2f);
            float rotateDuration = duration * _rotationDuarationRelativeToEntireJump;
            float height = _jumpHeight * Mathf.Clamp(distance / 5f, 0.5f, 1.5f);

            Vector3 peak = (startPos + localTarget) / 2 + Vector3.up * height;

            Vector3[] path = new Vector3[] { startPos, peak, localTarget };

            transform.DOLocalPath(path, duration, PathType.CatmullRom)
                .SetEase(_jumpEase)
                .OnComplete(() =>
                {
                    AnimatingNow = false;
                });

            _animationHandler.DOLocalRotate(new Vector3(0, 0, -360f * _fullRotationsOnJump), rotateDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _animationHandler.localRotation = Quaternion.Euler(0, 0, 0);
                });
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_thisSquareMovingNow)
                return;

            // The last collision has priority
            if (other.gameObject.TryGetComponent<RightField>(out var rightFieldContainer))
            {
                _lastAddingSquareResult = rightFieldContainer.GetCanAddSquareStatus(this);

                if (_lastAddingSquareResult == EAddingSquareResult.success)
                {
                    _rightFieldContainer = rightFieldContainer;
                    AnimateCanAddSquare();
                    InfoMessageService.Instance.ShowMessage(_localizationService.Localize("#TipCreateNewTower")).Forget();
                }
            }
            else if (Tower == null
                     && other.gameObject.TryGetComponent<SquareForBuilding>(out var otherSquareForBuilding)
                     && otherSquareForBuilding.Tower != null)
            {
                _lastAddingSquareResult = otherSquareForBuilding.Tower.GetCanAddSquareStatus(this);

                if (_lastAddingSquareResult == EAddingSquareResult.success)
                {
                    _otherSquaresNearby.Add(otherSquareForBuilding);
                    AnimateCanAddSquare();
                    InfoMessageService.Instance.ShowMessage(_localizationService.Localize("#TipAddSquareToTower")).Forget();
                }
            }
            else if (other.gameObject.TryGetComponent<BlackHole>(out var blackHoleContainer))
            {
                _lastAddingSquareResult = blackHoleContainer.GetCanAddSquareStatus(this);

                if (_lastAddingSquareResult == EAddingSquareResult.success)
                {
                    _blackHoleContainer = blackHoleContainer;
                    AnimateCanAddSquare();
                    InfoMessageService.Instance.ShowMessage(_localizationService.Localize("#TipDeleteSquare")).Forget();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_thisSquareMovingNow)
                return;
                
            if (other.gameObject.TryGetComponent<RightField>(out var rightFieldContainer))
            {
                _rightFieldContainer = null;

                if (_otherSquaresNearby.Count == 0)
                {
                    AnimateCannotAddSquare();
                    InfoMessageService.Instance.CloseMessage();
                }
            }
            else if (other.gameObject.TryGetComponent<SquareForBuilding>(out var otherSquareForBuilding)
                     && _otherSquaresNearby.Contains(otherSquareForBuilding))
            {
                _otherSquaresNearby.Remove(otherSquareForBuilding);

                if (_otherSquaresNearby.Count == 0 && _rightFieldContainer == null)
                {
                    AnimateCannotAddSquare();
                    InfoMessageService.Instance.CloseMessage();
                }
            }
            else if (other.gameObject.TryGetComponent<BlackHole>(out var blackHoleContainer))
            {
                _blackHoleContainer = null;

                AnimateCannotAddSquare();
                InfoMessageService.Instance.CloseMessage();
            }

            if (_otherSquaresNearby.Count == 0 && _rightFieldContainer == null && _blackHoleContainer == null)
                _lastAddingSquareResult = EAddingSquareResult.fail_squareInEmptyPlace;
        }

        private void AnimateCanAddSquare()
        {
            if (_thisSquareMovingNow)
            {
                _animationHandler
                    .DORotate(new Vector3(0, 0, 15f), _canAddAnimationDuration);
            }
        }

        private void AnimateCannotAddSquare()
        {
            if (_thisSquareMovingNow)
            {
                _animationHandler
                    .DORotate(Vector3.zero, _canAddAnimationDuration);
            }
        }

        public void MoveToPoint(Vector2 target, Action callBackOnComplete, bool withAnimation = true)
        {
            _currentLocalPositionTarget = target;

            if (withAnimation)
            {
                AnimatingNow = true;
                transform
                    .DOLocalMove(target, _moveDuration)
                    .SetEase(_moveEase)
                    .OnComplete(() =>
                    {
                        transform.localPosition = target;
                        AnimatingNow = false;
                        callBackOnComplete?.Invoke();
                    });
            }
            else
            {
                transform.localPosition = target;
                callBackOnComplete?.Invoke();
            }
        }
        
        public void StartFallAnimation(float localTargetY, float fallDuration, Ease fallEase)
        {
            AnimatingNow = true;
            FallAnimationNow = true;
            
            Vector2 target = new Vector2(transform.localPosition.x, localTargetY);
            
            _currentLocalPositionTarget = target;
            
            transform
                .DOLocalMove(target, fallDuration)
                .SetEase(fallEase)
                .OnComplete(() =>
                {
                    transform.localPosition = target;
                    AnimatingNow = false;
                    FallAnimationNow = false;
                });
        }
        
        public void DestroySquare(bool withAnimation)
        {
            if (Tower != null)
            {
                Tower.DestroyGhostSquare();
                Tower.RemoveSquare(this).Forget();
            }

            if (withAnimation)
            {
                AnimatingNow = true;
                _rotationTweener = _animationHandler
                    .DORotate(new Vector3(0, 0, -360f), _rotatingOnDestroyDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);

                _animationHandler
                    .DOScale(Vector3.zero, _destroyAnimationDuration)
                    .OnComplete(() =>
                    {
                        _rotationTweener.Kill();
                        AnimatingNow = false;
                        Destroy(gameObject);
                    });
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}