using Prefabs;
using UniRx;
using UnityEngine;
using Zenject;

namespace Services.Monobehs
{
    public class SquareMoveService : MonoBehaviour
    {
        public static SquareMoveService Instance => _instance ??= FindObjectOfType<SquareMoveService>();
        private static SquareMoveService _instance;
        
        public ReactiveProperty<bool> MovingSquareNow { get; private set; } = new ReactiveProperty<bool>(false);
        
        [SerializeField] private SquareForBuilding _squareForBuildingPrefab;
        [SerializeField] private Transform _squareInHandPrefabHandler;
        
        [SerializeField] private Camera _camera;
        
        [Inject] private DiContainer _diContainer;
        
        private SquareForBuilding _squareInHand;
        
        public void SpawnNewSquareInHand(Color newSquareColor)
        {
            if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                SquareForBuilding newSquare = _diContainer
                    .InstantiatePrefab(_squareForBuildingPrefab, _squareInHandPrefabHandler)
                    .GetComponent<SquareForBuilding>();
            
                newSquare.Init(newSquareColor);
                newSquare.InitMovingSubscribe(MovingSquareNow);
            
                _squareInHand = newSquare;
            }
        }
        
        public void AddExistingSquareInHand(SquareForBuilding square)
        {
            square.InitMovingSubscribe(MovingSquareNow);
            
            square.transform.SetParent(_squareInHandPrefabHandler);
            
            _squareInHand = square;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    Follow(touch.position);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    EndDrag();
            }
            else
            {
                if (Input.GetMouseButton(0))
                    Follow(Input.mousePosition);

                if (Input.GetMouseButtonUp(0))
                    EndDrag();
            }
        }

        private void Follow(Vector2 screenPos)
        {
            if (_squareInHand == null)
            {
                MovingSquareNow.Value = false;
                return;
            }
            
            MovingSquareNow.Value = true;

            _squareInHand.transform.position = ScreenToWorld(screenPos);
        }

        private void EndDrag()
        {
            MovingSquareNow.Value = false;
            
            if (_squareInHand == null) 
                return;

            _squareInHand.TryAddToLastSquareContainer();
            _squareInHand = null;
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 pos = screenPos;
            pos.z = Mathf.Abs(_camera.transform.position.z);
            return _camera.ScreenToWorldPoint(pos);
        }
    }
}