// Main.cs
using System.IO;
using BepInEx;
using UnityEngine;

namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.28.0")]
    public class Main : BaseUnityPlugin
    {
        private ConfigManager _configManager; //配置加载
        private GameComponentManager _gameComponentManager; //游戏组件管理
        private PlayerStatusController _playerStatusController; //核心逻辑
        private DGLabApiClient _apiClient; //API客户端
        private UIManager _uiManager;
        private bool _originalCursorVisibleState;
        private CursorLockMode _originalCursorLockState;

        public void Awake()
        {
            _apiClient = new DGLabApiClient(Logger);
            _configManager = new ConfigManager(this.Config);
            _gameComponentManager = new GameComponentManager(Logger);
            _playerStatusController = new PlayerStatusController(_configManager, _apiClient, Logger);
            // 初始化 UI 管理器
            _uiManager = new UIManager(_configManager);

            Logger.LogInfo("DGLAB 插件已加载，按 F10 打开设置菜单。");
        }

        public void Start()
        {
            // 首次尝试缓存游戏组件
            _gameComponentManager.CacheGameComponents();
        }

        public void Update()
        {
            if (Input.GetKeyDown(_configManager.ToggleUiKey.Value))
            {
                // 切换 UI 的可见性
                _uiManager.IsVisible = !_uiManager.IsVisible;

                if (_uiManager.IsVisible)
                {
                    // 1. 保存游戏当前的鼠标状态
                    _originalCursorVisibleState = Cursor.visible;
                    _originalCursorLockState = Cursor.lockState;

                    // 2. 强制显示并解锁鼠标，以便操作 UI
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = _originalCursorVisibleState;
                    Cursor.lockState = _originalCursorLockState;
                }
            }

            // 检查组件是否已准备就绪
            if (!_gameComponentManager.AreComponentsReady())
            {
                // 如果没准备好，就尝试再次获取
                _gameComponentManager.CacheGameComponents();

                if (!_gameComponentManager.AreComponentsReady())
                {
                    return; 
                }
            }

            // 执行核心逻辑
            _playerStatusController.ProcessPlayerStatusUpdate(_gameComponentManager);
        }
        public void OnGUI()
        {
            _uiManager.OnGUI();
        }
    }
}