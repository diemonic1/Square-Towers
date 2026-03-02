using System;
using System.Collections.Generic;
using Prefabs.SquareContainers;
using Services.SavingService;
using Services.SavingService.SavingModels;
using UnityEngine;
using Zenject;

namespace Prefabs.Fields
{
    public class RightField : SquareContainers.SquareContainer
    {
        [SerializeField] private Transform _pointToStartHeight;
        
        [SerializeField] private RectTransform _selfRectTransform;
        [SerializeField] private BoxCollider2D _collider;

        [SerializeField] private float _deadzoneOfSquareInput = 0.8f;
        
        [SerializeField] private Tower _towerPrefab;
        [SerializeField] private SquareForBuilding _squareForBuildingPrefab;
        
        [Inject] private DiContainer _diContainer;
        [Inject] private SavingService _savingService;

        private List<Tower> _spawnedTowers = new List<Tower>();

        private float _realHeight;
        
        public override EAddingSquareResult GetCanAddSquareStatus(SquareForBuilding square)
        {
            // You can remove this check and collect several towers

            if (_spawnedTowers.Count == 0)
                return EAddingSquareResult.success;
            else 
                return EAddingSquareResult.fail_rightFieldAlreadyHasTower;
        }
        
        private void Awake()
        {
            UpdateColliderSize();
        }

        private void UpdateColliderSize()
        {
            _realHeight = _selfRectTransform.rect.size.y;
            _collider.size = _selfRectTransform.rect.size * _deadzoneOfSquareInput;
            _collider.offset = new Vector2(-(_selfRectTransform.rect.size.x / 2), 0f);
        }

        public void RemoveTower(Tower tower)
        {
            if (_spawnedTowers.Contains(tower))
            {
                _spawnedTowers.Remove(tower);
                
                if (tower.gameObject != null)
                    Destroy(tower.gameObject);
            }
        }
        
        public override void AddSquare(SquareForBuilding square, bool withAnimation = true)
        {
            Tower tower = _diContainer
                .InstantiatePrefab(_towerPrefab, transform)
                .GetComponent<Tower>();

            _spawnedTowers.Add(tower);

            tower.Init(_realHeight, this);
            
            square.transform.SetParent(transform);
            
            Vector3 targetToJump = new Vector3(
                    square.transform.localPosition.x, 
                    _pointToStartHeight.localPosition.y, 
                    0
                );

            tower.transform.localPosition = targetToJump;
            
            tower.AddSquare(square, withAnimation);
        }

        public void RestoreFieldState()
        {
            FieldStateForSave fieldStateForSave = _savingService.Load();

            if (fieldStateForSave != null)
            {
                foreach (var towerForSave in fieldStateForSave.Towers)
                {
                    Tower tower = _diContainer
                        .InstantiatePrefab(_towerPrefab, transform)
                        .GetComponent<Tower>();

                    _spawnedTowers.Add(tower);

                    tower.Init(_realHeight, this);

                    Vector3 towerPosition = new Vector3(
                        towerForSave.Squares[0].Position.x, 
                        _pointToStartHeight.localPosition.y, 
                        0
                    );
            
                    tower.transform.localPosition = towerForSave.Position;

                    foreach (var squareForSave in towerForSave.Squares)
                    {
                        SquareForBuilding squareForBuilding = _diContainer
                            .InstantiatePrefab(_squareForBuildingPrefab, transform)
                            .GetComponent<SquareForBuilding>();
                
                        squareForBuilding.Init(squareForSave.Color);
            
                        tower.RestoreSquare(squareForBuilding, squareForSave.Position);
                    }
                }
            }
        }

        public void SaveFieldState()
        {
            List<TowerForSave> towersForSave = new();
            
            foreach (var tower in _spawnedTowers)
                towersForSave.Add(tower.GetTowerForSave());
            
            FieldStateForSave fieldStateForSave = new FieldStateForSave(towersForSave);
            
            _savingService.Save(fieldStateForSave);
        }
    }
}