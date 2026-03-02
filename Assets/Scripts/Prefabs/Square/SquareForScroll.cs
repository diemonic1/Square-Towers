using DG.Tweening;
using HelpfulScripts;
using Services.ConfigsService;
using Services.Monobehs;
using UnityEngine;
using Zenject;

namespace Prefabs
{
    public class SquareForScroll : Square
    {
        [Inject] private ConfigsService _configsService;
        
        public override void Init(Color color)
        {
            base.Init(color);
            
            _button.SetFunctionToButtonDownAndHold(
                () =>
                {
                    _animationHandler
                        .DOScale(_scaleToOnSelect, (float)_configsService.GameConfig.HoldingTimeToGetSquareFromScrollInMs / 1000)
                        .OnComplete(() =>
                        {
                            _animationHandler.DOScale(Vector3.one, _durationOfScaleRevert);
                        });
                },
                () =>
                {
                    SquareMoveService.Instance.SpawnNewSquareInHand(color);
                }, 
                _configsService.GameConfig.HoldingTimeToGetSquareFromScrollInMs);
        }
    }
}