using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI manacostText;

        private AbilityData ability;
        private int slotIndex;

        public event Action<int> OnClicked;

        public void Init(AbilityData abilityData, int index)
        {
            ability = abilityData;
            slotIndex = index;

            if (icon != null)
                icon.sprite = ability.icon;


            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnClicked?.Invoke(slotIndex));
            }
            if (manacostText != null)
            {
                manacostText.text = ability.ManaCost.ToString();
            }
        }


        public AbilityData GetAbility() => ability;
    }
}
