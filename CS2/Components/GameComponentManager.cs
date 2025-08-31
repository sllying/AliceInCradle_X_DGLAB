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

        public M2Attackable HpComponentAttackable { get; private set; } // ���� FireMode 0
        public PRNoel PrNoelComponent { get; private set; } // ���� HP (ģʽ 1/2) �� MP
        public M2MoverPr EpComponent { get; private set; }// ���� EP
        public PR PrComponent { get; private set; } // ���ڸ߳�

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