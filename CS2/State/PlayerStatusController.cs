// PlayerStatusController.cs
using System;
using HarmonyLib;

namespace AliceInCradle
{
    public class PlayerStatusController
    {
        private readonly ConfigManager _config;
        private readonly DGLabApiClient _apiClient;
        private readonly BepInEx.Logging.ManualLogSource _logger;
        // 状态追踪变量
        private int? _previousHp = null;
        private int? _previousMp = null;
        private int? _previousEp = null;
        private int? _previousOr = null;

        private bool _epFlag = false;
        private bool _orgasmFlag = false;
        private DateTime _orgasmDurationTimer;
        private DateTime _strengthReductionTimer;

        private const int ADD_CHANGE_LIMIT = -1000;
        public PlayerStatusController(ConfigManager config, DGLabApiClient apiClient, BepInEx.Logging.ManualLogSource logger)
        {
            _config = config;
            _apiClient = apiClient;
            _logger = logger;
        }
        // 核心逻辑处理
        public void ProcessPlayerStatusUpdate(GameComponentManager components)
        {

            if (components == null || !components.AreComponentsReady())
            {
                // 如果传入的组件管理器本身是 null 或者组件不完整，直接返回
                return;
            }


            // 获取当前玩家状态
            var (hp, hpMax) = GetHp(components);
            var (mp, mpMax) = GetMp(components);
            var ep = Traverse.Create(components.EpComponent).Field("ep").GetValue<int>();
            var orgasmCount = components.PrComponent.EpCon.getOrgasmedTotal();

            // 处理状态变化
            ProcessHpChange(hp, hpMax);
            ProcessMpChange(mp, mpMax);
            ProcessEpChange(ep);
            ProcessOrgasmChange(orgasmCount);
            ProcessOrgasmCooldown();
            ProcessIdleStrengthReduction();

            // 更新上一次的状态
            _previousHp = hp;
            _previousMp = mp;
            _previousEp = ep;
            _previousOr = orgasmCount;
        }
        // HP获取
        private (int, int) GetHp(GameComponentManager components)
        {
            if (_config.FireMode.Value == 0 && components.HpComponentAttackable != null)
            {
                int hp = Traverse.Create(components.HpComponentAttackable).Field("hp").GetValue<int>();
                int maxHp = Traverse.Create(components.HpComponentAttackable).Field("maxhp").GetValue<int>();
                return (hp, maxHp);
            }
            if ((_config.FireMode.Value == 1 || _config.FireMode.Value == 2) && components.PrNoelComponent != null)
            {
                int hp = Traverse.Create(components.PrNoelComponent).Field("hp").GetValue<int>();
                int maxHp = Traverse.Create(components.PrNoelComponent).Field("maxhp").GetValue<int>();
                return (hp, maxHp);
            }
            return (0, 0);
        }
        // MP获取
        private (int, int) GetMp(GameComponentManager components)
        {
            if (components.PrNoelComponent != null)
            {
                int mp = Traverse.Create(components.PrNoelComponent).Field("mp").GetValue<int>();
                int maxMp = Traverse.Create(components.PrNoelComponent).Field("maxmp").GetValue<int>();
                return (mp, maxMp);
            }
            return (0, 0);
        }
        // HP变化检测与处理
        private void ProcessHpChange(int currentHp, int maxHp)
        {
            if (_previousHp == null) _previousHp = maxHp;

            int difference = currentHp - _previousHp.Value;

            //_logger.LogInfo($"玩家变化了 {Math.Abs(difference)} 点 HP。");

            if (difference > 10 && difference < _config.MaxChange.Value) // 增加血量
            {
                _apiClient.SendStrengthUpdateAsync(sub: Math.Abs(difference)).ConfigureAwait(false);
            }
            else if (difference < 0 && difference > ADD_CHANGE_LIMIT) // 减少血量
            {
                int addAmount = Math.Abs((int)Math.Ceiling(difference * _config.HpReductionMultiplier.Value));
                _apiClient.SendStrengthUpdateAsync(add: addAmount).ConfigureAwait(false);
            }
        }
        // MP变化检测与处理
        private void ProcessMpChange(int currentMp, int maxMp)
        {
            if (_previousMp == null) _previousMp = maxMp;

            int difference = currentMp - _previousMp.Value;

            if (difference > 20 && difference < _config.MaxChange.Value) // 增加MP
            {
                _apiClient.SendStrengthUpdateAsync(sub: Math.Abs(difference)).ConfigureAwait(false);
            }
            else if (difference < 0 && difference > ADD_CHANGE_LIMIT) // 减少MP
            {
                bool specialCondition = difference <= -1 && difference > -10 && _config.Lowest.Value != 0 && _epFlag;

                if (_config.FireMode.Value == 2)
                {
                    if (specialCondition)
                    {
                        _apiClient.SendStrengthUpdateAsync(add: _config.Lowest.Value).ConfigureAwait(false);
                        _epFlag = false;
                    }
                    else if (difference <= -10)
                    {
                        int addAmount = Math.Abs((int)Math.Ceiling(difference * _config.MpReductionMultiplier.Value));
                        _apiClient.SendStrengthUpdateAsync(add: addAmount).ConfigureAwait(false);
                    }
                }
                else if (_config.FireMode.Value < 2 && specialCondition)
                {
                    _apiClient.SendStrengthUpdateAsync(add: _config.Lowest.Value).ConfigureAwait(false);
                    _epFlag = false;
                }
            }
        }
        // EP变化检测与处理
        private void ProcessEpChange(int currentEp)
        {
            if (_previousEp == null) _previousEp = currentEp;

            int difference = currentEp - _previousEp.Value;
            if (difference > 0 && difference < _config.MaxChange.Value)
            {
                _epFlag = true;
                int addAmount = Math.Abs((int)Math.Round(difference * _config.EpReductionMultiplier.Value / 10));
                _apiClient.SendStrengthUpdateAsync(add: addAmount).ConfigureAwait(false);
            }
        }
        // 高潮检测与处理
        private void ProcessOrgasmChange(int currentOrgasmCount)
        {
            if (_previousOr == null) _previousOr = currentOrgasmCount;

            int difference = currentOrgasmCount - _previousOr.Value;
            if (difference > 0)
            {
                int addAmount = Math.Abs((int)Math.Ceiling(difference * _config.Hero.Value * 1.0));
                _apiClient.SendStrengthUpdateAsync(add: addAmount).ConfigureAwait(false);
                _orgasmFlag = true;
                _orgasmDurationTimer = DateTime.UtcNow;
            }
        }
        // 高潮后的冷却处理
        private void ProcessOrgasmCooldown()
        {
            if (_orgasmFlag && (DateTime.UtcNow - _orgasmDurationTimer > TimeSpan.FromMilliseconds(_config.HoldMs.Value)))
            {
                _orgasmFlag = false;
                _apiClient.SendStrengthUpdateAsync(sub: _config.EroH.Value).ConfigureAwait(false);
            }
        }

        // 空闲时的强度减少
        private void ProcessIdleStrengthReduction()
        {
            if (DateTime.UtcNow - _strengthReductionTimer > TimeSpan.FromMilliseconds(_config.CheckIntervalMs.Value))
            {
                _strengthReductionTimer = DateTime.UtcNow;
                _apiClient.SendStrengthUpdateAsync(sub: _config.ReductionValue.Value).ConfigureAwait(false);
            }
        }
    }
}