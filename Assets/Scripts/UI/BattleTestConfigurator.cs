using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class BattleTestConfigurator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private AbilityData[] availableAbilities;

        [Header("UI Containers")]
        [SerializeField] private Transform teamAContainer;
        [SerializeField] private Transform teamBContainer;
        [SerializeField] private GameObject unitATeamPrefab; // prefab UI элемента юнита
        [SerializeField] private GameObject unitBTeamPrefab; // prefab UI элемента юнита
        [SerializeField] private Button addUnitATButton;
        [SerializeField] private Button addUnitBTButton;
        [SerializeField] private Button startBattleButton;

        public List<UnitUIEntry> teamAEntries = new();
        public List<UnitUIEntry> teamBEntries = new();

        private void Awake()
        {
            addUnitATButton.onClick.AddListener(() => AddUnitUI(teamAContainer, teamAEntries, unitATeamPrefab));
            addUnitBTButton.onClick.AddListener(() => AddUnitUI(teamBContainer, teamBEntries, unitBTeamPrefab));
            startBattleButton.onClick.AddListener(StartBattle);
        }

        private void AddUnitUI(Transform container, List<UnitUIEntry> entryList, GameObject prefab)
        {
            var go = Instantiate(prefab, container);
            var entry = go.GetComponent<UnitUIEntry>();
            entryList.Add(entry);
            entry.Setup(availableAbilities, () => entryList.Remove(entry));
        }

        private void StartBattle()
        {
            var teamAList = new List<BattleCharacter>();
            var teamBList = new List<BattleCharacter>();

            foreach (var entry in teamAEntries)
                teamAList.Add(CreateCharacterFromUI(entry));

            foreach (var entry in teamBEntries)
                teamBList.Add(CreateCharacterFromUI(entry));

            battleManager.SetTeams(teamAList, teamBList);
            battleManager.StartBattle();
        }

        private BattleCharacter CreateCharacterFromUI(UnitUIEntry entry)
        {
            var character = Instantiate(entry.BattlePrefab);

            if (character.CurrentStats != null)
            {
                character.CurrentStats.MaxHP = entry.hp;
                character.CurrentStats.CurrentHP = entry.hp;
                character.CurrentStats.MaxMana = entry.mana;
                character.CurrentStats.CurrentMana = entry.mana;
                character.CurrentStats.baseDamage = entry.baseDamage;
            }

            character.Abilities.Clear();
            foreach (var ability in entry.GetSelectedAbilities())
                character.Abilities.Add(ability);

            return character;
        }
    }
}
