using System;
using System.Threading.Tasks;
using AliceInCradle.Components;
using AliceInCradle.Config;
using AliceInCradle.Network;
using AliceInCradle.Tools;
using UnityEngine;

namespace AliceInCradle.State
{
    public class GameState : MonoBehaviour
    {
        private readonly ConfigManager _config;
        private readonly ApiClient _apiClient;
        private readonly GameComponents _components;

        private int? _previousHp;
        private int? _previousMp;
        private int? _previousEp;
        private int? _previousOr;
        private DateTime _strengthReductionTimer;
        private DateTime _orgasmDurationTimer;
        private bool _epFlag;
        private bool _orFlag;
        private bool _orFlag2;
        private int _endCD;

        private const int AddChangeLimit = -1000;

        public GameState(ConfigManager config, ApiClient apiClient, GameComponents components)
        {
            _config = config;
            _apiClient = apiClient;
            _components = components;
        }

        public async Task UpdateState()
        {
            if (!_components.AreComponentsValid())
            {
                return;
            }

            await UpdateOrgasmState();
            await UpdateEpState();
            await UpdateHpState();
            await UpdateMpState();
        }

        private async Task UpdateOrgasmState()
        {
            var prNoel = _components.PrComponent;
            MyLog.Log($"EndCD: {_endCD}");
            if (prNoel != null)
            {
                var epManager = prNoel.EpCon;
                if (epManager != null)
                {
                    int multiple = epManager.getOrgasmedTotal();

                    if (_previousOr == null)
                    {
                        _previousOr = multiple;
                    }
                    else
                    {
                        int difference = multiple - _previousOr.Value;
                        if (difference > 0)
                        {
                            int addDGLAB = Math.Abs((int)Math.Ceiling(difference * _config.Settings.Hero * 1.0));
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = addDGLAB });
                            _orFlag = true;
                            _orgasmDurationTimer = DateTime.UtcNow;
                        }

                        _previousOr = multiple;
                    }
                }
            }

            if (_orFlag)
            {
                DateTime now = DateTime.UtcNow;
                if (now - _orgasmDurationTimer > TimeSpan.FromMilliseconds(_config.Settings.HoldMs))
                {
                    _orFlag2 = true;
                }
                if (_orFlag2)
                {
                    _orFlag = false;
                    _orFlag2 = false;
                    await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Sub = _config.Settings.EroH });
                }
            }
        }

        private async Task UpdateEpState()
        {
            var ep = _components.EpComponent;
            if (ep != null)
            {
                var epNow = HarmonyLib.Traverse.Create(ep).Field("ep").GetValue<int>();
                if (_previousEp == null)
                {
                    _previousEp = epNow;
                }
                else
                {
                    int difference = epNow - _previousEp.Value;
                    if (difference > 0 && difference > _config.Settings.MaxChange)
                    {
                        _epFlag = true;
                        int addDGLAB = Math.Abs((int)Math.Round(difference * _config.Settings.EpReductionMultiplier / 10));
                        await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = addDGLAB });
                    }
                }
                _previousEp = epNow;
            }
        }

        private async Task UpdateHpState()
        {
            var (hp, hpMax) = _components.GetHpComponents(_config.Settings.FireMode);
            var (hpNow, hpStart) = _components.GetHpValues(hp, hpMax);

            if (_previousHp == null)
            {
                _previousHp = hpStart;
            }
            else
            {
                int difference = hpNow - _previousHp.Value;
                bool hpChanged = false;

                switch (_config.Settings.FireMode)
                {
                    case 0:
                    case 1:
                    case 2:
                        if (difference > 10 && difference > _config.Settings.MaxChange)
                        {
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Sub = Math.Abs(difference) });
                            hpChanged = true;
                        }
                        else if (difference < 0 && difference > AddChangeLimit)
                        {
                            int addDGLAB = Math.Abs((int)Math.Ceiling(difference * _config.Settings.HpReductionMultiplier));
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = addDGLAB });
                            hpChanged = true;
                        }
                        break;
                }

                if (!hpChanged)
                {
                    await CheckNoChangeState();
                }
                _previousHp = hpNow;
            }
        }

        private async Task UpdateMpState()
        {
            var (mpNow, mpStart) = _components.GetMpValues();

            if (_previousMp == null)
            {
                _previousMp = mpStart;
            }
            else
            {
                int difference = mpNow - _previousMp.Value;
                bool mpChanged = false;

                switch (_config.Settings.FireMode)
                {
                    case 0:
                    case 1:
                        if (difference > 20 && difference > _config.Settings.MaxChange)
                        {
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Sub = Math.Abs(difference) });
                            mpChanged = true;
                        }
                        else if (difference <= -1 && difference > -10 && _config.Settings.Lowest != 0 && _epFlag)
                        {
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = _config.Settings.Lowest });
                            _epFlag = false;
                            mpChanged = true;
                        }
                        break;

                    case 2:
                        if (difference > 20 && difference > _config.Settings.MaxChange)
                        {
                            await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Sub = Math.Abs(difference) });
                            mpChanged = true;
                        }
                        else if (difference < 0 && difference > AddChangeLimit)
                        {
                            if (difference <= -1 && difference > -10 && _config.Settings.Lowest != 0 && _epFlag)
                            {
                                await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = _config.Settings.Lowest });
                                _epFlag = false;
                            }
                            else if (difference <= -10)
                            {
                                int addDGLAB = Math.Abs((int)Math.Ceiling(difference * _config.Settings.MpReductionMultiplier));
                                await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Add = addDGLAB });
                            }
                            mpChanged = true;
                        }
                        break;
                }

                if (!mpChanged)
                {
                    await CheckNoChangeState();
                }
                _previousMp = mpNow;
            }
        }

        private async Task CheckNoChangeState()
        {
            await _apiClient.SendStrengthConfigAsync(new StrengthConfig());
            DateTime now = DateTime.UtcNow;
            if (now - _strengthReductionTimer > TimeSpan.FromMilliseconds(_config.Settings.CheckIntervalMs))
            {
                _strengthReductionTimer = now;
                _apiClient.SetEndCD(1);
                await _apiClient.SendStrengthConfigAsync(new StrengthConfig { Sub = _config.Settings.ReductionValue });
                _apiClient.SetEndCD(0);
            }
        }
    }
}
