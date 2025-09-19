using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class SuperHeroUI : MonoBehaviour
    {
        private SuperHero superHero;

        [Header("Mana UI")]
        [SerializeField] private Image manaSlider;
        [SerializeField] private TextMeshProUGUI manaText;

        [Header("Skills")]
        [SerializeField] private AbilitySlotUI abilitySlotprefab;
        [SerializeField] private Transform abilitiesContainer;

        private List<AbilitySlotUI> abilitySlots = new();

        public void Setup(SuperHero hero)
        {
            superHero = hero;

            InitSkills();
            UpdateManaUI();
        }

        private void Update()
        {
            if (superHero == null) return;
        }

        private void InitSkills()
        {
            var skills = superHero.superSkills;

            for (int i = 0; i < skills.Count; i++)
            {
                var abilitySlot = Instantiate(abilitySlotprefab, abilitiesContainer.transform).GetComponent<AbilitySlotUI>();
                abilitySlot.Init(skills[i], i);
                abilitySlot.OnClicked += OnSkillClicked;
                abilitySlots.Add(abilitySlot);
            }
        }

        private void OnSkillClicked(int index)
        {
            if (superHero == null) return;

            superHero.UseSkill(index);
            UpdateManaUI();
        }

        public void UpdateManaUI()
        {
            if (superHero == null) return;
            manaSlider.fillAmount = (float)superHero.currentMana / superHero.maxMana;
            manaText.text = $"{superHero.currentMana}/{superHero.maxMana}";

            for (int i = 0; i < superHero.superSkills.Count && i < abilitySlots.Count; i++)
            {
                var ability = superHero.superSkills[i];
                var btn = abilitySlots[i].GetComponent<Button>();
                if (btn != null)
                    btn.interactable = superHero.currentMana >= ability.ManaCost;
            }
        }


        public void ResetUI()
        {
            foreach (var slot in abilitySlots)
            {
                if (slot != null)
                {
                    slot.OnClicked -= OnSkillClicked;
                    Destroy(slot.gameObject);
                }
            }

            abilitySlots.Clear();

            superHero = null;

            manaSlider.fillAmount = 0f;
            manaText.text = "0/0";
        }
    }
}
