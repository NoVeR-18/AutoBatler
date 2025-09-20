using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleSystem
{
    public class BattleCharacter : MonoBehaviour
    {
        public string characterName;
        public BattleTeam Team;
        public CharacterStats CurrentStats;
        public DamagePopup damagePopupPrefab;
        public BarUI HPMPBarInstance;
        public List<AbilityData> Abilities = new();

        private Dictionary<AbilityData, float> cooldowns = new();
        private Dictionary<StatusEffect, float> statusEffects = new();

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool IsAlive => CurrentStats.CurrentHP > 0;

        public Sprite Portrait
        {
            get
            {
                if (spriteRenderer != null)
                    return spriteRenderer.sprite;
                else
                    return null;
            }
        }

        private void Awake()
        {
            if (damagePopupPrefab == null)
                damagePopupPrefab = Resources.Load<DamagePopup>("UI/DamagePopUp");

            if (healthBarPrefab == null)
                healthBarPrefab = Resources.Load<GameObject>("UI/HealthBar");
            if (animator == null)
                animator = GetComponent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            cooldowns = Abilities.ToDictionary(a => a, a => 0f);


            var go = Instantiate(healthBarPrefab, transform);
            go.transform.localPosition = new Vector3(0, 1.5f, 0);
            HPMPBarInstance = go.GetComponent<BarUI>();
            HPMPBarInstance.SetHealth(CurrentStats.CurrentHP, CurrentStats.MaxHP);
            HPMPBarInstance.SetMana(CurrentStats.CurrentMana, CurrentStats.MaxMana);
            if (Team == BattleTeam.Enemies)
            {
                HPMPBarInstance.SetColor(Color.red);
            }
        }

        public AbilityData GetNextReadyAbility(BattleManager manager)
        {
            foreach (var ability in Abilities)
            {
                if (cooldowns.TryGetValue(ability, out float remaining) && remaining <= 0)
                {

                    return ability;
                }
            }

            return null;
        }

        public void TakeDamage(int amount)
        {
            int remainingDamage = amount;

            if (remainingDamage != 0)
            {
                ShowDamagePopup(remainingDamage);
                CurrentStats.CurrentHP = Mathf.Max(0, CurrentStats.CurrentHP - remainingDamage);
                HPMPBarInstance?.SetHealth(CurrentStats.CurrentHP, CurrentStats.MaxHP);
                PlayHitReaction();
            }

            if (CurrentStats.CurrentHP <= 0)
            {

                Die();
            }
        }
        public void ShowShieldPopup(int shieldValue)
        {
            if (!damagePopupPrefab) return;

            var popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            popup.Setup(shieldValue, Color.blue);
        }

        public async void Die()
        {
            Debug.Log($"{characterName} has been defeated!");
            if (animator && HasParameter("Die", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("Die");
                await Task.Delay(TimeSpan.FromSeconds(GetAnimationLength()));
            }
        }

        private float GetAnimationLength()
        {
            return animator ? animator.GetCurrentAnimatorStateInfo(0).length : 0.5f;
        }

        public void PlayAttackAnimation(string skillTrigger = "Attack")
        {
            if (animator && HasParameter(skillTrigger, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(skillTrigger);
            else
                FlashColor(Color.yellow, 0.2f);
        }

        public void PlayHitReaction()
        {
            if (animator && HasParameter("Hit", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("Hit");
            else
                FlashColor(Color.red, 0.2f);
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

        public void ApplyStatusEffect(List<StatusEffect> newEffects)
        {
            foreach (var newEffect in newEffects)
            {
                bool isComboApplied = false;

                foreach (var existing in statusEffects.Keys.ToList())
                {
                    foreach (var combo in existing.comboEffects)
                    {
                        if (combo.otherEffect == newEffect)
                        {
                            ApplyComboEffect(existing, newEffect, combo);
                            isComboApplied = true;
                            break;
                        }
                    }
                    if (isComboApplied) break;

                    foreach (var combo in newEffect.comboEffects)
                    {
                        if (combo.otherEffect == existing)
                        {
                            ApplyComboEffect(existing, newEffect, combo);
                            isComboApplied = true;
                            break;
                        }
                    }
                    if (isComboApplied) break;
                }

                if (isComboApplied) continue;

                if (statusEffects.ContainsKey(newEffect))
                    statusEffects[newEffect] = newEffect.Duration;
                else
                {
                    statusEffects.Add(newEffect, newEffect.Duration);
                }
            }
        }

        private void ApplyComboEffect(StatusEffect a, StatusEffect b, StatusCombo combo)
        {
            if (combo.removeOriginals)
            {
                statusEffects.Remove(a);
                statusEffects.Remove(b);
            }

            if (combo.resultingEffect != null)
            {
                statusEffects[combo.resultingEffect] = combo.resultingEffect.Duration;
            }
        }

        public void StatusEffectTick(float deltaTime)
        {
            var expired = new List<StatusEffect>();

            foreach (var effect in statusEffects.Keys.ToList())
            {
                statusEffects[effect] -= deltaTime;


                if (effect.Type == StatusType.Affliction && effect.damagePerTick > 0)
                    TakeDamage(effect.damagePerTick);

                if (effect.regenHP && effect.regenAmount > 0)
                    Heal((int)effect.regenAmount);

                if (statusEffects[effect] <= 0)
                {
                    expired.Add(effect);
                }
            }

            foreach (var effect in expired)
                statusEffects.Remove(effect);

        }

        public void Heal(int amount)
        {
            if (statusEffects.Keys.Any(e => e.regenHP)) return;
            CurrentStats.CurrentHP = Mathf.Min(CurrentStats.CurrentHP + amount, CurrentStats.MaxHP);
            HPMPBarInstance?.SetHealth(CurrentStats.CurrentHP, CurrentStats.MaxHP);
        }

        private void ShowDamagePopup(int damage)
        {
            if (!damagePopupPrefab) return;

            var popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up + (Vector3.right * UnityEngine.Random.Range(-1f, 1f)), Quaternion.identity);
            if (damage < 0)
            {
                damage = -damage;
                popup.Setup(damage, Color.green);
            }
            else
                popup.Setup(damage);

        }

        public bool CanAct() => !statusEffects.Keys.Any(e => e.preventAction || e.sleep || e.isFeared);

        public bool CanUseMagic() => !statusEffects.Keys.Any(e => e.preventMagic || e.silence);

        public bool CanUsePhysical() => !statusEffects.Keys.Any(e => e.preventPhysical);

        public float GetCastSpeedMultiplier()
        {
            float multiplier = 1f;
            foreach (var e in statusEffects.Keys)
                multiplier *= e.castSpeedMultiplier;
            return multiplier;
        }

        public float GetMagicDamageMultiplier()
        {
            float multiplier = 1f;
            foreach (var e in statusEffects.Keys)
                multiplier *= e.magicDamageModifier;
            return multiplier;
        }

        public float GetPhysicalDamageMultiplier()
        {
            float multiplier = 1f;
            foreach (var e in statusEffects.Keys)
                multiplier *= e.physicalDamageModifier;
            return multiplier;
        }

        public List<StatusEffect> GetCurrentEffects() => statusEffects.Keys.ToList();

        public float GetRemainingCooldown(StatusEffect effect) =>
            statusEffects.TryGetValue(effect, out float t) ? t : 0f;

        public Dictionary<AbilityData, float> GetCooldowns() => cooldowns;
        public Dictionary<StatusEffect, float> GetStatusEffects() => statusEffects;
    }
}
