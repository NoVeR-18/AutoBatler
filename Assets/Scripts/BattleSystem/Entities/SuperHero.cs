using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BattleSystem
{
    public class SuperHero : MonoBehaviour
    {
        public BattleTeam Team;
        public int maxMana = 140;
        public int currentMana;
        public List<AbilityData> superSkills;
        public Dictionary<AbilityData, float> skillCooldowns = new();

        [SerializeField] private Animator animator;

        public System.Func<BattleTeam, List<BattleCharacter>> GetEnemiesFunc;
        public System.Func<BattleTeam, List<BattleCharacter>> GetAlliesFunc;

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            skillCooldowns = superSkills.ToDictionary(a => a, a => 0f);
        }

        public void GainMana(int amount)
        {
            currentMana = Mathf.Min(currentMana + amount, maxMana);
        }

        public void StartCooldown(AbilityData ability)
        {
            if (ability != null)
                skillCooldowns[ability] = ability.cooldown;
        }

        public void TickCooldowns(float deltaTime)
        {
            foreach (var key in skillCooldowns.Keys.ToList())
                skillCooldowns[key] = Mathf.Max(0, skillCooldowns[key] - deltaTime);
        }

        public void UseSkill(int index)
        {
            if (index < 0 || index >= superSkills.Count) return;

            var skill = superSkills[index];
            if (skill == null) return;

            if (currentMana < skill.ManaCost || skillCooldowns[skill] > 0)
                return;

            // выбираем цели
            var targets = SelectTargets(skill.TargetType);

            if (targets.Count > 0)
            {
                foreach (var target in targets)
                    target.TakeDamage(skill.GetDamage());

                currentMana -= skill.ManaCost;
                StartCooldown(skill);
                PlayAttackAnimation(skill.animationTrigger);
            }
        }
        public void PlayAttackAnimation(string skillTrigger = "Attack")
        {
            if (animator && HasParameter(skillTrigger, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(skillTrigger);
            else
                FlashColor(Color.yellow, 0.2f);
        }
        public void FlashColor(Color flashColor, float duration)
        {
            if (gameObject != null)
                foreach (var r in GetComponentsInChildren<SpriteRenderer>())
                {
                    var originalColor = r.color;
                    r.DOColor(flashColor, 0.05f).OnComplete(() => r.DOColor(originalColor, duration));
                }
        }
        public bool HasParameter(string name, AnimatorControllerParameterType type)
        {
            return animator != null && animator.parameters.Any(p => p.name == name && p.type == type);
        }
        private List<BattleCharacter> SelectTargets(AbilityTargetType targetType)
        {
            List<BattleCharacter> allies = GetAlliesFunc?.Invoke(Team).Where(c => c.IsAlive).ToList() ?? new();
            List<BattleCharacter> enemies = GetEnemiesFunc?.Invoke(Team).Where(c => c.IsAlive).ToList() ?? new();

            switch (targetType)
            {
                case AbilityTargetType.SingleEnemy:
                    return enemies.Count > 0 ? new List<BattleCharacter> { enemies[Random.Range(0, enemies.Count)] } : new();
                case AbilityTargetType.SingleAlly:
                    var possibleAllies = allies.Where(x => x != null && x.IsAlive).ToList();
                    return possibleAllies.Count > 0 ? new List<BattleCharacter> { possibleAllies[Random.Range(0, possibleAllies.Count)] } : new();
                case AbilityTargetType.Self:
                    return new List<BattleCharacter> { null }; // тут можно сделать сам супергерой, если у него будет HP
                case AbilityTargetType.AllEnemies:
                    return enemies;
                case AbilityTargetType.AllAllies:
                    return allies;
                case AbilityTargetType.All:
                    return enemies.Concat(allies).ToList();
                default:
                    return new();
            }
        }

        public Dictionary<AbilityData, float> GetCooldowns() => skillCooldowns;
    }
}