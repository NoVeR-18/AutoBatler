using BattleSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class BattlePreparationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleSpawnData spawnData;

        [Header("UI")]
        [SerializeField] private Transform playerUnitsContainer; // панель снизу для юнитов
        [SerializeField] private UnitDraggableUI unitUIPrefab;
        [SerializeField] private Button startBattleButton;

        public List<BattleParticipantData> PlayerTeam;
        public List<BattleParticipantData> EnemyTeam = new();
        public SuperParticipantData superHero;

        private List<UnitDraggableUI> playerUnitUIs = new();
        private SuperHero superHeroInstance;

        private void Awake()
        {
            startBattleButton.onClick.AddListener(StartBattle);
            battleManager.OnDefeat += () => gameObject.SetActive(true);
        }

        private void Start()
        {
            List<BattleCharacter> PlayerUnits = new List<BattleCharacter>();
            foreach (var unit in PlayerTeam)
            {
                if (unit.battleCharacter != null)
                {
                    unit.nameID = unit.battleCharacter.characterName;
                    unit.stats = unit.battleCharacter.CurrentStats;
                    unit.abilities = unit.battleCharacter.Abilities;
                    PlayerUnits.Add(unit.battleCharacter);
                }
            }
            List<BattleCharacter> EnemyUnits = new List<BattleCharacter>();
            foreach (var unit in EnemyTeam)
            {
                if (unit.battleCharacter != null)
                {
                    unit.nameID = unit.battleCharacter.characterName;
                    unit.stats = unit.battleCharacter.CurrentStats;
                    unit.abilities = unit.battleCharacter.Abilities;
                    EnemyUnits.Add(unit.battleCharacter);
                }
            }


            Setup(PlayerUnits, EnemyUnits);
        }

        public void Setup(List<BattleCharacter> playerUnits, List<BattleCharacter> enemyUnits)
        {
            // очистим старые UI
            foreach (var ui in playerUnitUIs)
                Destroy(ui.gameObject);
            playerUnitUIs.Clear();

            // создаём UI для каждого юнита игрока
            foreach (var unit in playerUnits)
            {
                var ui = Instantiate(unitUIPrefab, playerUnitsContainer);
                unit.Team = BattleTeam.Alias;
                ui.Setup(unit);
                playerUnitUIs.Add(ui);
            }

            // расставляем противников сразу на их спавнпоинты
            var enemySpawns = spawnData.GetSpawnPoints()[BattleTeam.Enemies];
            for (int i = 0; i < enemyUnits.Count && i < enemySpawns.Count; i++)
            {
                var unit = Instantiate(enemyUnits[i], enemySpawns[i].transform);
                unit.Team = BattleTeam.Enemies;
                unit.transform.localPosition = Vector3.zero;
                enemySpawns[i].AssignUnitDirect(unit);
            }

            var spawn = spawnData.superHeroSpawn;
            var heroInstance = Instantiate(superHero.superHero, spawn.transform);
            heroInstance.maxMana = superHero.maxMana;
            heroInstance.currentMana = 0;
            heroInstance.Team = BattleTeam.Alias;
            heroInstance.superSkills = superHero.abilities;
            heroInstance.transform.localPosition = Vector3.zero;
            superHeroInstance = heroInstance;
        }


        private void StartBattle()
        {
            // Собираем команды из спавнпоинтов
            var teamAList = CollectTeam(BattleTeam.Alias);
            var teamBList = CollectTeam(BattleTeam.Enemies);

            // выключаем UI настройки
            gameObject.SetActive(false);

            // Передаём собранные команды в менеджер
            battleManager.SetTeams(teamAList, teamBList, superHeroInstance);
        }

        private List<BattleCharacter> CollectTeam(BattleTeam team)
        {
            var list = new List<BattleCharacter>();
            var spawnPoints = spawnData.GetSpawnPoints()[team];

            foreach (var sp in spawnPoints)
            {
                var unitUI = sp.GetAssignedCharacter();
                if (unitUI != null)
                {
                    list.Add(unitUI);
                }
            }

            return list;
        }
    }
}

[System.Serializable]
public class BattleParticipantData
{
    public BattleCharacter battleCharacter;
    [HideInInspector] public string nameID;
    public BattleTeam team;
    public CharacterStats stats;
    public List<AbilityData> abilities = new();

}
[System.Serializable]
public class SuperParticipantData
{
    public SuperHero superHero;
    [HideInInspector] public string nameID;
    public BattleTeam team;
    public int maxMana = 140;
    public List<AbilityData> abilities = new();

}