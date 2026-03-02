using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace Services.Monobehs
{
    public class InfoMessageService : MonoBehaviour
    {
        public static InfoMessageService Instance => _instance ??= FindObjectOfType<InfoMessageService>();
        private static InfoMessageService _instance;
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Transform _textHandler;
        
        [SerializeField] private Transform _showPoint;
        [SerializeField] private Transform _closePoint;
        
        [SerializeField] private float _moveDuration;
        [SerializeField] private Ease _moveEase;
        
        private bool _showMessageNow;
        private bool _waitingForCloseMessage;
        
        private TweenerCore<Vector3, Vector3, VectorOptions> _moveTween;
        
        private CancellationTokenSource _cancellationTokenSource;
        
        public async UniTask ShowMessage(string message, int autoHideAfterMS = -1)
        {
            if (_waitingForCloseMessage)
                _cancellationTokenSource.Cancel();
            
            if (!_showMessageNow)
            {
                _moveTween = _textHandler
                    .DOLocalMove(_showPoint.localPosition, _moveDuration)
                    .SetEase(_moveEase);
            }
            
            _showMessageNow = true;
            _text.text = message;

            if (autoHideAfterMS > 0)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                
                _waitingForCloseMessage = true;
                
                await UniTask.Delay(autoHideAfterMS, cancellationToken: _cancellationTokenSource.Token);
                
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    CloseMessage();
                    _waitingForCloseMessage = false;
                }
            }
        }

        public void CloseMessage()
        {
            _showMessageNow = false;
            
            _moveTween = _textHandler
                .DOLocalMove(_closePoint.localPosition, _moveDuration)
                .SetEase(_moveEase);
        }
    }
}