using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    [System.Serializable]
    public class StatusCombo
    {
        public StatusEffect otherEffect;
        public StatusEffect resultingEffect;
        public bool removeOriginals;
    }

    [CreateAssetMenu(menuName = "Battle/Status Effect")]
    public class StatusEffect : ScriptableObject
    {
        public string effectName;
        public Sprite Icon;
        public StatusType Type;
        public float Duration;
        public int damagePerTick;

        public float weaknessMultiplier = 1f;
        public bool preventAction;               // can't act at all
        public bool preventMagic;                // can't use magic
        public bool preventPhysical;             // can't use physical
        public bool silence;                     // don't cast spells
        public bool sleep;                       // sleep - skip turn, wake up on damage
        public bool isFeared;                    // panic - random move towards ally
        public bool forceAllyTargeting;          // only target allies
        public bool randomTargeting;             // random targeting
        public bool blockNextAbility;            // next ability is blocked
        public bool doubleMagic;                 // magic casts twice
        public bool doublePhysical;              // physical attacks twice
        public bool regenHP;                     // heal over time
        public int regenAmount;

        public float castSpeedMultiplier = 1f;   // 0.6f if Chilled
        public float magicDamageModifier = 1f;   // 0.8f if Mind Shackled
        public float physicalDamageModifier = 1f;// 0.8f if Weakened
        public float resistanceModifier = 1f;    // 0.8f if Shattered
        public List<StatusCombo> comboEffects = new(); // combo- reactions with other status effects

        public override bool Equals(object obj)
        {
            if (obj is not StatusEffect other) return false;
            return effectName == other.effectName;
        }

        public override int GetHashCode()
        {
            return effectName.GetHashCode();
        }
    }

}
