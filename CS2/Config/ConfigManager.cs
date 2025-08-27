using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace AliceInCradle.Config
{
    public class ConfigManager : MonoBehaviour
    {
        private readonly string _configPath;
        private readonly FileSystemWatcher _configWatcher;
        private readonly object _configLock = new object();

        public ConfigSettings Settings { get; private set; }

        public ConfigManager(string configPath)
        {
            _configPath = configPath;
            Settings = new ConfigSettings();
            _configWatcher = SetupConfigWatcher();
            LoadConfig();
        }

        private FileSystemWatcher SetupConfigWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(Path.GetFullPath(_configPath)),
                Filter = Path.GetFileName(_configPath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Changed += OnConfigFileChanged;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void LoadConfig()
        {
            lock (_configLock)
            {
                string jsonContent = File.ReadAllText(_configPath);
                JObject config = JObject.Parse(jsonContent);
                Settings.UpdateFromJson(config);
            }
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    LoadConfig();
                    Debug.Log("Configuration reloaded successfully");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reloading configuration: {ex.Message}");
                }
            });
        }

        public void Dispose()
        {
            _configWatcher?.Dispose();
        }
    }
}
