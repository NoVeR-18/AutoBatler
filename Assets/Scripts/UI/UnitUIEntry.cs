using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class UnitUIEntry : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_InputField hpInput;
        public TMP_InputField manaInput;
        public TMP_InputField baseDamageInput;
        public Transform abilitiesContainer;
        public GameObject abilityTogglePrefab;
        public Button removeButton;
        public BattleCharacter BattlePrefab;

        private List<AbilityData> availableAbilities = new();
        private List<Toggle> abilityToggles = new();

        public int hp => int.TryParse(hpInput.text, out int val) ? val : 100;
        public int mana => int.TryParse(manaInput.text, out int val) ? val : 50;
        public int baseDamage => int.TryParse(baseDamageInput.text, out int val) ? val : 10;

        public void Setup(AbilityData[] abilities, Action onRemove)
        {
            availableAbilities = new List<AbilityData>(abilities);

            foreach (Transform child in abilitiesContainer)
                Destroy(child.gameObject);

            abilityToggles.Clear();

            foreach (var ability in abilities)
            {
                var go = Instantiate(abilityTogglePrefab, abilitiesContainer);
                var toggle = go.GetComponent<Toggle>();
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = ability.abilityName;
                abilityToggles.Add(toggle);
            }
            hpInput.text = "100";
            manaInput.text = "100";
            baseDamageInput.text = "5";

            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() =>
            {
                onRemove?.Invoke();
                Destroy(gameObject);
            });
        }

        public List<AbilityData> GetSelectedAbilities()
        {
            var list = new List<AbilityData>();
            for (int i = 0; i < abilityToggles.Count; i++)
            {
                if (abilityToggles[i].isOn)
                    list.Add(availableAbilities[i]);
            }
            return list;
        }
    }
}
