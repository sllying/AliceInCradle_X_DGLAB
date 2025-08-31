// UIManager.cs
using System;
using UnityEngine;

namespace AliceInCradle
{
    public class UIManager
    {
        public bool IsVisible { get; set; } = false;

        private readonly ConfigManager _config;
        private Rect _windowRect = new Rect(20, 20, 450, 600); // 调整了窗口大小以容纳所有选项

        // 用于临时存储输入框内容的字符串变量
        private string _heroStr, _holdMsStr, _eroHStr;
        private string _hpMultiplierStr, _mpMultiplierStr, _epMultiplierStr;
        private string _maxChangeStr, _checkIntervalStr, _reductionValueStr, _lowestStr;

        public UIManager(ConfigManager config)
        {
            _config = config;
            // 初始化时，将当前的配置值转换为字符串，用于显示在输入框中
            _heroStr = config.Hero.Value.ToString();
            _holdMsStr = config.HoldMs.Value.ToString();
            _eroHStr = config.EroH.Value.ToString();
            _hpMultiplierStr = config.HpReductionMultiplier.Value.ToString("0.0#"); // 格式化浮点数
            _mpMultiplierStr = config.MpReductionMultiplier.Value.ToString("0.0#");
            _epMultiplierStr = config.EpReductionMultiplier.Value.ToString("0.0#");
            _maxChangeStr = config.MaxChange.Value.ToString();
            _checkIntervalStr = config.CheckIntervalMs.Value.ToString();
            _reductionValueStr = config.ReductionValue.Value.ToString();
            _lowestStr = config.Lowest.Value.ToString();
        }

        public void OnGUI()
        {
            if (!IsVisible) return;
            // 绘制主窗口
            _windowRect = GUILayout.Window(12346, _windowRect, DrawWindow, $"DGLAB 插件设置 (按 {_config.ToggleUiKey.Value} 关闭)");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // --- 模式设置 ---
            GUILayout.Label("核心模式\r\n1 默认，受到伤害，增加兴奋度，高潮时增加强度\r\n2 困难，同上默认但消耗mp时增加强度 注：包括使用魔法\r\n0 彩蛋，同上默认但所有伤害都增加强度 注：包括造成的伤害");
            GUILayout.Label($"当前模式: {_config.FireMode.Value} ({GetFireModeDescription(_config.FireMode.Value)})");
            _config.FireMode.Value = (int)GUILayout.HorizontalSlider(_config.FireMode.Value, 0, 2);
            GUILayout.Space(15);

            // --- 高潮设置 ---
            GUILayout.Label("高潮设置 (注意连续高潮会叠加)");
            DrawIntField("高潮增加强度:", ref _heroStr, _config.Hero);
            DrawIntField("效果持续时间(ms):", ref _holdMsStr, _config.HoldMs);
            DrawIntField("结束后恢复强度:", ref _eroHStr, _config.EroH);
            GUILayout.Space(15);

            // --- 倍率设置 ---
            GUILayout.Label("强度倍率");
            DrawFloatField("HP减少转化倍率（0.1 为 10滴血增加1强度）:", ref _hpMultiplierStr, _config.HpReductionMultiplier);
            DrawFloatField("MP减少转化倍率（0.1 为 10滴蓝增加1强度）:", ref _mpMultiplierStr, _config.MpReductionMultiplier);
            DrawFloatField("EP增加转化倍率（1 为 10兴奋度增加1强度，会与上叠加，建议设置在1以下）:", ref _epMultiplierStr, _config.EpReductionMultiplier);
            DrawIntField("强度生效上限（例一次扣超过200血/蓝/兴奋度时不会提升强度）:", ref _maxChangeStr, _config.MaxChange);
            GUILayout.Space(15);

            // --- 强度自然衰减 ---
            GUILayout.Label("强度自然衰减");
            DrawIntField("衰减间隔(ms):", ref _checkIntervalStr, _config.CheckIntervalMs);
            DrawIntField("每次衰减值:", ref _reductionValueStr, _config.ReductionValue);
            GUILayout.Space(15);

            // --- 实验性功能 ---
            GUILayout.Label("实验性功能");
            DrawIntField("(已废弃)最低增强值:", ref _lowestStr, _config.Lowest);

            // 占位符，将关闭按钮推到底部
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("关闭菜单"))
            {
                IsVisible = false;
            }

            GUILayout.EndVertical();
            GUI.DragWindow(); // 让窗口可以拖动
        }

        private string GetFireModeDescription(int mode)
        {
            switch (mode)
            {
                case 0: return "彩蛋";
                case 1: return "默认";
                case 2: return "困难";
                default: return "未知";
            }
        }

        // --- 绘制UI元素的辅助方法 ---
        private void DrawFloatField(string label, ref string valueStr, BepInEx.Configuration.ConfigEntry<float> configEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            valueStr = GUILayout.TextField(valueStr);
            if (float.TryParse(valueStr, out float result))
            {
                configEntry.Value = result;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawIntField(string label, ref string valueStr, BepInEx.Configuration.ConfigEntry<int> configEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            valueStr = GUILayout.TextField(valueStr);
            if (int.TryParse(valueStr, out int result))
            {
                configEntry.Value = result;
            }
            GUILayout.EndHorizontal();
        }
    }
}