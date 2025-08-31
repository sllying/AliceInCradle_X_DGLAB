// Main.cs
using System.IO;
using BepInEx;
using UnityEngine;

namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.28.0")]
    public class Main : BaseUnityPlugin
    {
        private ConfigManager _configManager; //���ü���
        private GameComponentManager _gameComponentManager; //��Ϸ�������
        private PlayerStatusController _playerStatusController; //�����߼�
        private DGLabApiClient _apiClient; //API�ͻ���
        private UIManager _uiManager;
        private bool _originalCursorVisibleState;
        private CursorLockMode _originalCursorLockState;

        public void Awake()
        {
            _apiClient = new DGLabApiClient(Logger);
            _configManager = new ConfigManager(this.Config);
            _gameComponentManager = new GameComponentManager(Logger);
            _playerStatusController = new PlayerStatusController(_configManager, _apiClient, Logger);
            // ��ʼ�� UI ������
            _uiManager = new UIManager(_configManager);

            Logger.LogInfo("DGLAB ����Ѽ��أ��� F10 �����ò˵���");
        }

        public void Start()
        {
            // �״γ��Ի�����Ϸ���
            _gameComponentManager.CacheGameComponents();
        }

        public void Update()
        {
            if (Input.GetKeyDown(_configManager.ToggleUiKey.Value))
            {
                // �л� UI �Ŀɼ���
                _uiManager.IsVisible = !_uiManager.IsVisible;

                if (_uiManager.IsVisible)
                {
                    // 1. ������Ϸ��ǰ�����״̬
                    _originalCursorVisibleState = Cursor.visible;
                    _originalCursorLockState = Cursor.lockState;

                    // 2. ǿ����ʾ��������꣬�Ա���� UI
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = _originalCursorVisibleState;
                    Cursor.lockState = _originalCursorLockState;
                }
            }

            // �������Ƿ���׼������
            if (!_gameComponentManager.AreComponentsReady())
            {
                // ���û׼���ã��ͳ����ٴλ�ȡ
                _gameComponentManager.CacheGameComponents();

                if (!_gameComponentManager.AreComponentsReady())
                {
                    return; 
                }
            }

            // ִ�к����߼�
            _playerStatusController.ProcessPlayerStatusUpdate(_gameComponentManager);
        }
        public void OnGUI()
        {
            _uiManager.OnGUI();
        }
    }
}