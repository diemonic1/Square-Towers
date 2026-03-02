using Cysharp.Threading.Tasks;
using Services.ConfigsService.ScriptableObjectsConfigs;
using Zenject;

namespace Services.ConfigsService
{
    public class ConfigsService : IConfigsService
    {
        public LocalizationConfig LocalizationConfig => _localizationConfig;
        private LocalizationConfig _localizationConfig;
        
        public GameConfig GameConfig => _gameConfigInstance;
        private GameConfig _gameConfigInstance;
        public bool ConfigsLoaded { get; private set; }
        
        [Inject] private GameConfig _gameConfig;
        
        public async UniTask LoadConfigs()
        {
            // Configs can be downloaded from the server

            _gameConfigInstance = _gameConfig;

            _localizationConfig = new LocalizationConfig();
            
            ConfigsLoaded = true;
        }
    }
}