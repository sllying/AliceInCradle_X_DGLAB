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
            // ��ʼ�����ù���������Ϸ״̬��API�ͻ���
            _configManager = new ConfigManager("Config.json");
            //��ʼ����Ϸ���
            _gameComponents = new GameComponents();
            //��ʼ��API�ͻ���
            _apiClient = new ApiClient();
            //��ʼ����Ϸ״̬
            _gameState = new GameState(_configManager, _apiClient, _gameComponents);
            //������Ϸ���
            _gameComponents.CacheComponents();
        }

        public void Update()
        {
            if (!_gameComponents.AreComponentsValid())
            {
                _gameComponents.CacheComponents();
            }
            // ÿ֡������Ϸ״̬
            _gameState.UpdateState().ConfigureAwait(false);
        }

        public void OnDestroy()
        {
            _configManager?.Dispose();
        }
    }
}
