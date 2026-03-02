using Cysharp.Threading.Tasks;
using DG.Tweening;
using Prefabs.SquareContainers;
using Unity.VisualScripting;
using UnityEngine;

namespace Prefabs
{
    public class BlackHole : SquareContainer
    {
        [SerializeField] private Transform _handlerForDeletingSquare;
        [SerializeField] private Transform _pointToMoveDeletingSquare;

        [SerializeField] private Transform _animationHandler;

        [SerializeField] private Vector3 _bigScale;
        [SerializeField] private Vector3 _normalScale;
        [SerializeField] private float _canAddAnimationDuration;
        
        public override EAddingSquareResult GetCanAddSquareStatus(SquareForBuilding square)
        {
            return EAddingSquareResult.success;
        }

        public override void AddSquare(SquareForBuilding square, bool withAnimation = true)
        {
            if (withAnimation)
            {
                square.transform.SetParent(_handlerForDeletingSquare);

                if (square.Tower != null)
                {
                    square.Tower.DestroyGhostSquare();
                    square.Tower.RemoveSquare(square).Forget();
                }
            
                square.MoveToPoint(
                    new Vector2(square.transform.localPosition.x, _pointToMoveDeletingSquare.transform.localPosition.y),
                    () =>
                    {
                        square.DestroySquare(false);
                    });
            }
            else
            {
                square.DestroySquare(false);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<SquareForBuilding>(out var otherSquareForBuilding)
                && GetCanAddSquareStatus(otherSquareForBuilding) == EAddingSquareResult.success)
            {
                AnimateCanAddSquare();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<SquareForBuilding>(out var otherSquareForBuilding)
                && GetCanAddSquareStatus(otherSquareForBuilding) == EAddingSquareResult.success)
            {
                AnimateCannotAddSquare();
            }
        }
        
        private void AnimateCanAddSquare()
        {
            _animationHandler
                .DOScale(_bigScale, _canAddAnimationDuration);
        }

        private void AnimateCannotAddSquare()
        {
            _animationHandler
                .DOScale(_normalScale, _canAddAnimationDuration);
        }
    }
}