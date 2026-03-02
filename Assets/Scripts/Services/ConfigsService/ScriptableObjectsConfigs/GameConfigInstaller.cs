using UnityEngine;
using Zenject;

namespace Services.ConfigsService.ScriptableObjectsConfigs
{
    [CreateAssetMenu(fileName = "GameConfigInstaller", menuName = "Installers/GameConfigInstaller")]
    public class GameConfigInstaller : ScriptableObjectInstaller<GameConfigInstaller>
    {
        [SerializeField] private GameConfig _gameConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameConfig).AsSingle();
        }
    }
}