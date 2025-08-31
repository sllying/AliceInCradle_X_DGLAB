// ConfigManager.cs
using System;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AliceInCradle
{
    public class ConfigManager : IDisposable
    {
        private readonly BepInEx.Logging.ManualLogSource _logger;
        // 公开的配置属性，供其他类访问
        public float HpReductionMultiplier { get; private set; }
        public float MpReductionMultiplier { get; private set; }
        public float EpReductionMultiplier { get; private set; }
        public int CheckIntervalMs { get; private set; }
        public int ReductionValue { get; private set; }
        public int FireMode { get; private set; }
        public int Lowest { get; private set; }
        public int Hero { get; private set; }
        public int HoldMs { get; private set; }
        public int EroH { get; private set; }
        public int MaxChange { get; private set; }

        private readonly string _configPath;
        private FileSystemWatcher _configWatcher;
        private readonly object _configLock = new object();

        public ConfigManager(string configPath, BepInEx.Logging.ManualLogSource logger)
        {
            _configPath = configPath;
            _logger = logger;
            // 设置默认值，防止文件不存在或读取失败时出错
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            HpReductionMultiplier = 0f;
            MpReductionMultiplier =0f;
            EpReductionMultiplier = 0f;
            CheckIntervalMs = 0;
            ReductionValue = 0;
            FireMode = 0;
            Lowest = 0;
            Hero = 0;
            HoldMs = 0;
            EroH = 0;
            MaxChange = 0;
        }

        public void LoadConfig()
        {
            lock (_configLock)
            {
                if (!File.Exists(_configPath))
                {
                    Debug.LogError($"Configuration file not found at: {_configPath}");
                    return;
                }

                try
                {
                    string jsonContent = File.ReadAllText(_configPath);
                    JObject config = JObject.Parse(jsonContent);

                    HpReductionMultiplier = (float)config["hpReductionMultiplier"];
                    MpReductionMultiplier = (float)config["mpReductionMultiplier"];
                    EpReductionMultiplier = (float)config["epReductionMultiplier"];
                    CheckIntervalMs = (int)config["CheckIntervalMs"];
                    ReductionValue = (int)config["ReductionValue"];
                    FireMode = (int)config["FireMode"];
                    Lowest = (int)config["lowest"];
                    Hero = (int)config["Hero"];
                    HoldMs = (int)config["holdMs"];
                    EroH = (int)config["eroH"];
                    MaxChange = (int)config["maxChange"];
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error loading configuration: {ex.Message}");
                }
            }
        }

        public void SetupConfigWatcher()
        {
            _configWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_configPath),
                Filter = Path.GetFileName(_configPath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            _configWatcher.Changed += OnConfigFileChanged;
            _configWatcher.EnableRaisingEvents = true;
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                Debug.Log("Configuration file changed, reloading...");
                LoadConfig();
            });
        }

        public void Dispose()
        {
            if (_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Dispose();
            }
        }
    }
}