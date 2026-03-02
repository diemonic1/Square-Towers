

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Prefabs.Fields;
using Services.SavingService.SavingModels;
using UnityEngine;
using Zenject;

namespace Prefabs.SquareContainers
{
    public class Tower : SquareContainer
    {
        [SerializeField] private int _delayBetweenSquaresFallMS;
        [SerializeField] private float _squaresFallDuration;
        [SerializeField] private Ease _squaresFallEase;
        
        [SerializeField] private GameObject _ghostSquarePrefab;
        
        public bool IsAnySquareAnimatingNow => _squares.Any(s => s.AnimatingNow);
        
        private List<SquareForBuilding> _squares = new List<SquareForBuilding>();
        
        private Vector2? _squareSize = null;

        private float _fieldHeight;

        private GameObject _spawnedGhostSquare;
        
        private RightField _rightField;

        private SquareForBuilding _lasRemovedSquare;
        
        [Inject] private DiContainer _diContainer;
        
        public void SpawnGhostSquare(Vector2 position)
        {
            _spawnedGhostSquare = _diContainer.InstantiatePrefab(_ghostSquarePrefab, transform);
            _spawnedGhostSquare.transform.SetAsFirstSibling();
            _spawnedGhostSquare.transform.localPosition = position;
        }
            
        public void DestroyGhostSquare()
        {
            if (_spawnedGhostSquare != null)
                Destroy(_spawnedGhostSquare);
        }
            
        public void Init(float fieldHeight, RightField rightField)
        {
            _fieldHeight = fieldHeight;
            _rightField = rightField;
        }
        
        public override EAddingSquareResult GetCanAddSquareStatus(SquareForBuilding square)
        {
            _squareSize ??= square.GetSquareSize;
            
            if (_squares.Count == 0)
                return EAddingSquareResult.success;

            if ((_squares.Count + 1) * _squareSize.Value.y >= _fieldHeight)
                return EAddingSquareResult.fail_towerIsMaxHeight;
            
            return EAddingSquareResult.success;
        }

        public async UniTask RemoveSquare(SquareForBuilding square)
        {
            if (_squares.Contains(square) && _lasRemovedSquare != square)
            {
                _lasRemovedSquare = square;
                
                int index = _squares.IndexOf(square);

                for (int i = index + 1; i < _squares.Count; i++)
                {
                    _squareSize ??= square.GetSquareSize;
                    
                    _squares[i].StartFallAnimation(
                            _squares[i].transform.localPosition.y - _squareSize.Value.y,
                            _squaresFallDuration,
                            _squaresFallEase
                        );
                    
                    await UniTask.Delay(_delayBetweenSquaresFallMS);
                }

                _squares.Remove(square);
                
                if (_squares.Count == 0)
                    _rightField.RemoveTower(this);
                
                _rightField.SaveFieldState();
            }
        }

        public TowerForSave GetTowerForSave()
        {
            List<SquareForSave> squaresForSave = new();
            
            foreach (var square in _squares)
                squaresForSave.Add(square.GetSquareForSave());
                
            return new TowerForSave(squaresForSave, transform.localPosition);
        }
        
        public override void AddSquare(SquareForBuilding square, bool withAnimation = true)
        {
            _squareSize ??= square.GetSquareSize;
            
            square.SetTower(this);

            square.transform.SetParent(transform);
            
            if (withAnimation)
                square.AnimateJump(GetNextSquarePosition());
            else
                square.MoveToPoint(GetNextSquarePosition(), null, false);
            
            _squares.Add(square);
            _rightField.SaveFieldState();
        }

        public void RestoreSquare(SquareForBuilding square, Vector2 squareLocalPosition)
        {
            _squareSize ??= square.GetSquareSize;
            
            square.SetTower(this);

            square.transform.SetParent(transform);
            
            square.MoveToPoint(squareLocalPosition, null, false);
            
            _squares.Add(square);
        }
        
        private Vector3 GetNextSquarePosition()
        {
            if (_squares.Count == 0 || _squareSize == null)
                return Vector3.zero;
            
            float halfOfSquareWidth = _squareSize.Value.x / 2f;
            
            float xPosition = GetTopSquare().CurrentLocalPositionTarget.x +
                              Random.Range(-halfOfSquareWidth, halfOfSquareWidth);
            
            return new Vector3(xPosition, _squares.Count * _squareSize.Value.y, 0);
        }
        
        private SquareForBuilding GetTopSquare()
        {
            if (_squares == null || _squares.Count == 0)
                return null;
            
            return _squares[^1];
        }
    }
}