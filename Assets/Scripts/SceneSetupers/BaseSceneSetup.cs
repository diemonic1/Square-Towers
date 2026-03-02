using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Prefabs.Fields;
using Services.ConfigsService;
using Services.SavingService;
using Services.SavingService.SavingModels;
using UI;
using UnityEngine;
using Zenject;

namespace SceneSetupers
{
    public class BaseSceneSetup : MonoBehaviour
    {
        [SerializeField] private RightField _rightField;
        
        [Inject] private ConfigsService _configsService;
        
        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
        
            DeferredInit().Forget();
        }

        private async UniTask DeferredInit()
        {
            await UniTask.WaitWhile(() => _configsService == null);

            _configsService.LoadConfigs().Forget();
            
            await UniTask.WaitWhile(() => !_configsService.ConfigsLoaded);

            List<Color> colors = new List<Color>();
            
            foreach (var colorString in _configsService.GameConfig.SquareColorsHex)
            {
                colors.Add(Utils.Utils.HexToColor(colorString));
            }
            
            SquaresScrollHandler.Instance.SpawnSquaresInScroll(colors);

            _rightField.RestoreFieldState();
        }
    }
}