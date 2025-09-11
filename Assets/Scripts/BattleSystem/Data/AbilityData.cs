using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Battle/Ability")]
    public class AbilityData : ScriptableObject
    {
        public string abilityName;
        public Sprite icon;
        public int ManaCost;
        public float cooldown;
        public int baseDamage;
        public AbilityTargetType TargetType;
        public List<StatusEffect> effects; // status + duration etc.

        public string animationTrigger = "Attack";

        public int GetDamage()
        {
            return baseDamage;
        }
    }

    [Serializable]
    public class DamageClass
    {
        public DamageType damageType;
        public int baseDamage;
    }

}