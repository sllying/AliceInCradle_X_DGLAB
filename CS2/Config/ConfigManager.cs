using BepInEx.Configuration;
using UnityEngine; 

namespace AliceInCradle
{
    public class ConfigManager
    {
        // --- 公开的配置项 ---
        // BepInEx 会把这些设置保存到 BepInEx/config/AliceInCradle.DGLAB.cfg 文件中
        public ConfigEntry<int> FireMode { get; private set; }
        public ConfigEntry<int> Hero { get; private set; }
        public ConfigEntry<int> HoldMs { get; private set; }
        public ConfigEntry<int> EroH { get; private set; }
        public ConfigEntry<float> HpReductionMultiplier { get; private set; }
        public ConfigEntry<float> MpReductionMultiplier { get; private set; }
        public ConfigEntry<float> EpReductionMultiplier { get; private set; }
        public ConfigEntry<int> MaxChange { get; private set; }
        public ConfigEntry<int> CheckIntervalMs { get; private set; }
        public ConfigEntry<int> ReductionValue { get; private set; }
        public ConfigEntry<int> Lowest { get; private set; }
        public ConfigEntry<KeyCode> ToggleUiKey { get; private set; }

        public ConfigManager(ConfigFile config)
        {
            // 格式: Bind("分类", "名称", 默认值, "描述文字")
            // --- 模式设置 ---
            FireMode = config.Bind("1. 核心模式", "开火模式", 1,
                "1: 默认模式, 受伤增加兴奋度。\n" +
                "2: 困难模式, 在模式1基础上，消耗MP也会增加强度。\n" +
                "0: 彩蛋模式, 在模式1基础上，所有伤害（包括你造成的）都增加强度。");

            // --- 高潮设置 ---
            Hero = config.Bind("2. 高潮设置", "高潮增加强度", 10, "每次触发高潮事件时增加的强度值。设为0可关闭。");
            HoldMs = config.Bind("2. 高潮设置", "高潮持续时间 (ms)", 2000, "高潮增加强度后，效果的持续时间（毫秒）。");
            EroH = config.Bind("2. 高潮设置", "高潮后降低强度", 20, "高潮效果结束后，恢复（减少）的强度值。");

            // --- 倍率设置 ---
            HpReductionMultiplier = config.Bind("3. 强度倍率", "HP减少转化倍率", 0.3f, "每减少1点HP，转化为强度的倍率 (例如0.3代表增加0.3强度)。");
            MpReductionMultiplier = config.Bind("3. 强度倍率", "MP减少转化倍率", 0.1f, "每减少1点MP，转化为强度的倍率 (例如0.1代表增加0.1强度)。");
            EpReductionMultiplier = config.Bind("3. 强度倍率", "EP增加转化倍率", 0.7f, "每增加10点EP(兴奋度)，转化为强度的倍率 (例如0.7代表增加0.7强度)。");
            MaxChange = config.Bind("3. 强度倍率", "单次变化生效上限", 200, "单次状态变化（HP/MP/EP）的数值如果超过此上限，则不会触发强度变化，用以防止数据异常。");

            // --- 强度自然衰减 ---
            CheckIntervalMs = config.Bind("4. 强度衰减", "衰减间隔 (ms)", 1000, "无事件发生时，每隔多少毫秒减少一次强度。");
            ReductionValue = config.Bind("4. 强度衰减", "每次衰减值", 1, "每次衰减时减少的强度值。");

            // --- 实验性功能 ---
            Lowest = config.Bind("5. 实验性", "(已废弃)最低增强值", 0, "现已被兴奋度系统取代。如需使用会和兴奋度叠加。设为0关闭。");

            // --- 界面设置 ---
            ToggleUiKey = config.Bind("6. 界面", "开关界面热键", KeyCode.F10, "按下此键显示或隐藏设置菜单。");
        }
    }
}