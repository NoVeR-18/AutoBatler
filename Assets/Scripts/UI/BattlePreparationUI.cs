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

        public List<BattleCharacter> PlayerUnits;
        public List<BattleCharacter> EnemyUnits = new();

        private List<UnitDraggableUI> playerUnitUIs = new();

        private void Awake()
        {
            startBattleButton.onClick.AddListener(StartBattle);
            battleManager.OnDefeat += () => gameObject.SetActive(true);
        }

        private void Start()
        {
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
                ui.Setup(unit);
                playerUnitUIs.Add(ui);
            }

            // расставляем противников сразу на их спавнпоинты
            var enemySpawns = spawnData.GetSpawnPoints()[BattleTeam.Team2];
            for (int i = 0; i < enemyUnits.Count && i < enemySpawns.Count; i++)
            {
                enemySpawns[i].AssignUnitDirect(enemyUnits[i]);
            }
        }


        private void StartBattle()
        {
            // Собираем команды из спавнпоинтов
            var teamAList = CollectTeam(BattleTeam.Team1);
            var teamBList = CollectTeam(BattleTeam.Team2);

            // выключаем UI настройки
            gameObject.SetActive(false);

            // Передаём собранные команды в менеджер
            battleManager.SetTeams(teamAList, teamBList, null);
        }

        private List<BattleCharacter> CollectTeam(BattleTeam team)
        {
            var list = new List<BattleCharacter>();
            var spawnPoints = spawnData.GetSpawnPoints()[team];

            foreach (var sp in spawnPoints)
            {
                var unitUI = sp.GetAssignedUnit();
                if (unitUI != null)
                {
                    var character = unitUI.GetCharacter();
                    if (character != null)
                    {
                        list.Add(character);
                    }
                }
            }

            return list;
        }
    }
}
