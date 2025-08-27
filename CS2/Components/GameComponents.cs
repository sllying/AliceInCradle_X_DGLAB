using HarmonyLib;
using m2d;
using nel;
using UnityEngine;

namespace AliceInCradle.Components
{
    public class GameComponents : MonoBehaviour
    {
        public M2Attackable HpComponentAttackable { get; private set; }
        public PRNoel PrNoelComponent { get; private set; }
        public M2MoverPr EpComponent { get; private set; }
        public PR PrComponent { get; private set; }

        public void CacheComponents()
        {
            HpComponentAttackable = Object.FindObjectOfType<M2Attackable>();
            PrNoelComponent = Object.FindObjectOfType<PRNoel>();
            EpComponent = Object.FindObjectOfType<M2MoverPr>();
            PrComponent = Object.FindObjectOfType<PR>();
        }

        public bool AreComponentsValid()
        {
            return PrNoelComponent != null && EpComponent != null && PrComponent != null;
        }

        public (object hp, object hpMax) GetHpComponents(int fireMode)
        {
            if (fireMode == 0)
            {
                return (HpComponentAttackable, HpComponentAttackable);
            }
            return (PrNoelComponent, PrNoelComponent);
        }

        public (int current, int max) GetHpValues(object hp, object hpMax)
        {
            int current = hp != null ? Traverse.Create(hp).Field("hp").GetValue<int>() : 0;
            int max = hpMax != null ? Traverse.Create(hpMax).Field("maxhp").GetValue<int>() : 0;
            return (current, max);
        }

        public (int current, int max) GetMpValues()
        {
            if (PrNoelComponent == null) return (0, 0);
            int current = Traverse.Create(PrNoelComponent).Field("mp").GetValue<int>();
            int max = Traverse.Create(PrNoelComponent).Field("maxmp").GetValue<int>();
            return (current, max);
        }
    }
}
