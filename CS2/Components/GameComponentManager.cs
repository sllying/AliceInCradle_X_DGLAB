using UnityEngine;
using m2d;
using nel;
using nel.mgm;
using BepInEx.Logging;

namespace AliceInCradle
{
    public class GameComponentManager
    {
        private readonly BepInEx.Logging.ManualLogSource _logger;

        public GameComponentManager(ManualLogSource logger)
        {
            _logger = logger;
        }

        public M2Attackable HpComponentAttackable { get; private set; } // 用于 FireMode 0
        public PRNoel PrNoelComponent { get; private set; } // 用于 HP (模式 1/2) 和 MP
        public M2MoverPr EpComponent { get; private set; }// 用于 EP
        public PR PrComponent { get; private set; } // 用于高潮

        public bool AreComponentsReady()
        {
            return PrNoelComponent != null &&
                   EpComponent != null &&
                   PrComponent != null &&
                   HpComponentAttackable != null &&
                   PrComponent.EpCon != null;
        }

        public void CacheGameComponents()
        {
            HpComponentAttackable = Object.FindObjectOfType<M2Attackable>();
            PrNoelComponent = Object.FindObjectOfType<PRNoel>();
            EpComponent = Object.FindObjectOfType<M2MoverPr>();
            PrComponent = Object.FindObjectOfType<PR>();
        }
    }
}