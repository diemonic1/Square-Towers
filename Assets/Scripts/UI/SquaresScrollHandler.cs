using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Prefabs;
using Services.Monobehs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

namespace UI
{
    public class SquaresScrollHandler : MonoBehaviour
    {
        public static SquaresScrollHandler Instance => _instance ??= FindObjectOfType<SquaresScrollHandler>();
        private static SquaresScrollHandler _instance;

        [SerializeField] private Transform _squaresHandler;
        [SerializeField] private SquareForScroll _squareForScroll;
        
        [SerializeField] private ScrollRect _scrollRect;
        
        [Inject] private DiContainer _diContainer;

        private void Start()
        {
            SquareMoveService.Instance.MovingSquareNow.Subscribe(value =>
            {
                _scrollRect.enabled = !value;
            }).AddTo(this);
        }

        public void SpawnSquaresInScroll(List<Color> squaresColors)
        {
            foreach (var squareColor in squaresColors)
            {
                SquareForScroll squareForScroll = _diContainer
                    .InstantiatePrefab(_squareForScroll, _squaresHandler)
                    .GetComponent<SquareForScroll>();
                
                squareForScroll.Init(squareColor);
            }
        }
    }
}