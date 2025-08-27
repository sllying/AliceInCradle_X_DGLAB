using BepInEx;
using AliceInCradle.Config;
using AliceInCradle.Components;
using AliceInCradle.State;

namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.28.0")]
    public class Main : BaseUnityPlugin
    {
        private ConfigManager _configManager;
        private GameState _gameState;
        private ApiClient _apiClient;
        private GameComponents _gameComponents;

        public void Start()
        {
            // 初始化配置管理器、游戏状态和API客户端
            _configManager = new ConfigManager("Config.json");
            //初始化游戏组件
            _gameComponents = new GameComponents();
            //初始化API客户端
            _apiClient = new ApiClient();
            //初始化游戏状态
            _gameState = new GameState(_configManager, _apiClient, _gameComponents);
            //缓存游戏组件
            _gameComponents.CacheComponents();
        }

        public void Update()
        {
            if (!_gameComponents.AreComponentsValid())
            {
                _gameComponents.CacheComponents();
            }
            // 每帧更新游戏状态
            _gameState.UpdateState().ConfigureAwait(false);
        }

        public void OnDestroy()
        {
            _configManager?.Dispose();
        }
    }
}
