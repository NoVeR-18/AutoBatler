using System;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Button button;

        private AbilityData ability;
        private int slotIndex;

        public event Action<int> OnClicked;

        public void Init(AbilityData abilityData, int index)
        {
            ability = abilityData;
            slotIndex = index;

            if (icon != null)
                icon.sprite = ability.icon;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnClicked?.Invoke(slotIndex));
            }
        }

        public void UpdateCooldown(float remaining, float max)
        {
            if (cooldownOverlay == null) return;

            if (remaining > 0 && max > 0)
                cooldownOverlay.fillAmount = remaining / max;
            else
                cooldownOverlay.fillAmount = 0;
        }

        public AbilityData GetAbility() => ability;
    }
}
