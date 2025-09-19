using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleSpawnData spawnPoints;

        private List<BattleCharacter> teamA = new();
        private List<BattleCharacter> teamB = new();
        private SuperHero superHero;

        [Header("Timing")]
        public float actionInterval = 1f;
        private float speedMultiplier = 1f;
        private bool isPaused = false;
        private bool isBattleActive = false;

        [Header("UI")]

        public GameUI gameUI;
        private int _heroTeamRoundDamage;

        public Action OnDefeat;

        public int HeroTeamRoundDamage
        {
            get => _heroTeamRoundDamage;
            set
            {
                if (_heroTeamRoundDamage != value)
                {
                    _heroTeamRoundDamage = value;
                    gameUI?.SetDamage(_heroTeamRoundDamage);
                }
            }
        }
        private int _round;
        public int RoundNumber
        {
            get => _round;
            set
            {
                if (_round != value)
                {
                    _round = value;
                    gameUI?.SetRound(_round);
                }
            }
        }
        private Dictionary<BattleTeam, List<BattleSpawnPoint>> _spawnPoints;
        private Dictionary<BattleSpawnPoint, BattleCharacter> occupiedSpawns = new();

        private List<BattleCharacter> turnOrder = new();

        private void Start()
        {
            _spawnPoints = spawnPoints.GetSpawnPoints();

            gameUI.AddListenerSpeed(ToggleSpeed);
            gameUI.AddListenerSurender(Surrender);
            gameUI.AddListenerPause(TogglePause);
        }



        public void SetTeams(List<BattleCharacter> teamAList, List<BattleCharacter> teamBList, SuperHero superHero)
        {
            teamA = teamAList;
            teamB = teamBList;
            if (superHero != null)
                this.superHero = superHero;

            StartBattle();
        }

        public void StartBattle()
        {
            isBattleActive = true;
            if (gameUI != null)
                gameUI.gameObject.SetActive(true);
            RoundNumber = 1;
            occupiedSpawns.Clear();
            SpawnTeams();
            BuildTurnOrder();
            StopAllCoroutines();
            StartCoroutine(TurnLoop());
        }

        private void ToggleSpeed()
        {
            speedMultiplier = (Mathf.Approximately(speedMultiplier, 1f)) ? 2f : 1f;
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
        }

        private void Surrender()
        {
            foreach (var c in teamA)
                c.CurrentStats.CurrentHP = 0;
            foreach (var c in teamB)
                c.CurrentStats.CurrentHP = 0;

            isBattleActive = false;
            Defeat();

            Debug.Log("Battle Over (Surrender)");
        }

        private void SpawnTeams()
        {
            occupiedSpawns.Clear();

            // team A
            for (int i = 0; i < teamA.Count && i < _spawnPoints[BattleTeam.Team1].Count; i++)
            {
                var spawn = _spawnPoints[BattleTeam.Team1][i];
                var character = Instantiate(teamA[i], spawn.transform.position, Quaternion.identity);
                character.Team = BattleTeam.Team1;
                character.CurrentStats.CurrentMana = 0; // start with 0 mana
                occupiedSpawns[spawn] = character;
                teamA[i] = character;
            }

            // team B
            for (int i = 0; i < teamB.Count && i < _spawnPoints[BattleTeam.Team2].Count; i++)
            {
                var spawn = _spawnPoints[BattleTeam.Team2][i];
                var character = Instantiate(teamB[i], spawn.transform.position, Quaternion.identity);
                character.Team = BattleTeam.Team2;
                character.CurrentStats.CurrentMana = 0; // start with 0 mana
                occupiedSpawns[spawn] = character;
                teamB[i] = character;
            }

            // superhero
            if (superHero != null && spawnPoints.superHeroSpawn != null)
            {
                var spawn = spawnPoints.superHeroSpawn;
                var heroInstance = Instantiate(superHero, spawn.transform.position, Quaternion.identity);
                superHero = heroInstance;
                superHero.GetEnemiesFunc = team => team == BattleTeam.Team1 ? teamB : teamA;
                superHero.GetAlliesFunc = team => team == BattleTeam.Team1 ? teamA : teamB;
                superHero.currentMana = 0;
                superHero.OnDamageDealt += AddHeroDamage;
                if (gameUI.superHeroUI != null)
                    gameUI.superHeroUI.Setup(superHero);
            }
        }
        private void AddHeroDamage(int dmg)
        {
            HeroTeamRoundDamage += dmg;
        }

        private void BuildTurnOrder()
        {
            turnOrder.Clear();

            List<BattleSpawnPoint> aPoints = _spawnPoints.ContainsKey(BattleTeam.Team1) ? _spawnPoints[BattleTeam.Team1] : new();
            List<BattleSpawnPoint> bPoints = _spawnPoints.ContainsKey(BattleTeam.Team2) ? _spawnPoints[BattleTeam.Team2] : new();

            int max = Mathf.Max(aPoints.Count, bPoints.Count);

            for (int i = 0; i < max; i++)
            {
                if (i < aPoints.Count && occupiedSpawns.TryGetValue(aPoints[i], out var aChar) && aChar != null && aChar.IsAlive)
                    turnOrder.Add(aChar);

                if (i < bPoints.Count && occupiedSpawns.TryGetValue(bPoints[i], out var bChar) && bChar != null && bChar.IsAlive)
                    turnOrder.Add(bChar);
            }
        }

        public BattleSpawnPoint GetFreeSpawnPoint(BattleTeam team)
        {
            if (!_spawnPoints.ContainsKey(team)) return null;

            foreach (var point in _spawnPoints[team])
            {
                if (!occupiedSpawns.ContainsKey(point) || occupiedSpawns[point] == null || !occupiedSpawns[point].IsAlive)
                    return point;
            }

            return null;
        }

        private bool IsBattleOver()
        {
            return teamA.All(c => !c.IsAlive) || teamB.All(c => !c.IsAlive);
        }

        private List<BattleCharacter> GetAllCharacters()
        {
            return teamA.Concat(teamB).ToList();
        }

        private List<BattleCharacter> SelectTargets(BattleCharacter caster, AbilityTargetType targetType)
        {
            List<BattleCharacter> allies = GetAllCharacters().Where(c => c.Team == caster.Team && c.IsAlive).ToList();
            List<BattleCharacter> enemies = GetAllCharacters().Where(c => c.Team != caster.Team && c.IsAlive).ToList();

            switch (targetType)
            {
                case AbilityTargetType.SingleEnemy:
                    return GetRandomAliveTarget(enemies);
                case AbilityTargetType.SingleAlly:
                    var possibleAllies = allies.Where(x => x.IsAlive && x != caster).ToList();
                    return GetRandomAliveTarget(possibleAllies.Count > 0 ? possibleAllies : new List<BattleCharacter> { caster });
                case AbilityTargetType.Self:
                    return new List<BattleCharacter> { caster };
                case AbilityTargetType.AllEnemies:
                    return enemies;
                case AbilityTargetType.AllAllies:
                    return allies;
                case AbilityTargetType.All:
                    return GetAllCharacters().Where(x => x.IsAlive).ToList();
                default:
                    return new();
            }
        }

        private List<BattleCharacter> GetRandomAliveTarget(List<BattleCharacter> list)
        {
            if (list == null || list.Count == 0) return new();
            return new List<BattleCharacter> { list[UnityEngine.Random.Range(0, list.Count)] };
        }

        private void ApplyAbility(BattleCharacter caster, BattleCharacter target, AbilityData ability)
        {
            if (target == null || !target.IsAlive) return;

            if (ability == null) return;

            int totalDamage = ability.GetDamage();

            if (totalDamage > 0)
            {
                target.TakeDamage(totalDamage);

                if (caster.Team == superHero.Team)
                {
                    HeroTeamRoundDamage += totalDamage;
                }

                GrantMana(target, totalDamage / 2);
                GrantMana(caster, totalDamage / 2);
            }

            Debug.Log($"{caster.characterName} used {ability.abilityName} on {target.characterName}, dealing {totalDamage} damage!");



            if (ability.effects != null && ability.effects.Count > 0)
                target.ApplyStatusEffect(ability.effects);


        }

        private void CleanDeadCharactersAndFreeSpawns()
        {
            void CleanTeam(List<BattleCharacter> team)
            {
                for (int i = team.Count - 1; i >= 0; i--)
                {
                    var character = team[i];
                    if (!character.IsAlive)
                    {
                        FreeSpawnIfOccupied(character);
                        team.RemoveAt(i);
                        DOTween.Kill(character.gameObject); // stop all tweens
                        Destroy(character.gameObject);
                    }
                }
            }

            CleanTeam(teamA);
            CleanTeam(teamB);

            BuildTurnOrder();
        }

        private void FreeSpawnIfOccupied(BattleCharacter character)
        {
            if (occupiedSpawns == null) return;

            var spawn = occupiedSpawns.FirstOrDefault(kv => kv.Value == character).Key;

            if (spawn != null)
            {
                occupiedSpawns.Remove(spawn);
            }
        }

        private IEnumerator TurnLoop()
        {
            while (isBattleActive && !IsBattleOver())
            {
                if (turnOrder.Count == 0)
                {
                    BuildTurnOrder();
                    yield return null;
                    continue;
                }

                HeroTeamRoundDamage = 0;
                for (int i = 0; i < turnOrder.Count; i++)
                {
                    // пауза
                    while (isPaused) yield return null;

                    var current = turnOrder[i];

                    if (current == null || !current.IsAlive || !current.CanAct())
                    {
                        yield return new WaitForSeconds(actionInterval / speedMultiplier);
                        continue;
                    }

                    yield return StartCoroutine(ExecuteActionCoroutine(current));
                    CleanDeadCharactersAndFreeSpawns();
                    yield return new WaitForSeconds(actionInterval / speedMultiplier);
                }
                RoundNumber++;
                // end of round
            }
            Defeat();
        }

        private void Defeat()
        {
            isBattleActive = false;
            gameUI.ReserUI();
            OnDefeat?.Invoke();
            StopAllCoroutines();
            foreach (var c in GetAllCharacters())
            {
                Destroy(c.gameObject);
            }
            Destroy(superHero.gameObject);
        }


        private IEnumerator ExecuteActionCoroutine(BattleCharacter character)
        {
            var cooldowns = character.GetCooldowns();

            // find first ability that is off cooldown and can be used (enough mana, etc.)
            AbilityData chosen = null;
            foreach (var ability in character.Abilities)
            {
                if (!cooldowns.TryGetValue(ability, out float remaining)) continue;
                if (remaining > 0f) continue;
                // проверка на ману в CurrentStats (предполагается CurrentStats.CurrentMana)
                if (character.CurrentStats != null && ability.ManaCost > 0)
                {
                    if (character.CurrentStats.CurrentMana < ability.ManaCost)
                        continue;
                }

                chosen = ability;
                break;
            }

            if (chosen != null)
            {
                var targets = SelectTargets(character, chosen.TargetType);
                if (targets != null && targets.Count > 0)
                {
                    foreach (var t in targets)
                        ApplyAbility(character, t, chosen);
                }

                character.PlayAttackAnimation(chosen.animationTrigger);

                if (character.CurrentStats != null)
                {
                    if (chosen.ManaCost > 0)
                    {
                        character.CurrentStats.CurrentMana = Mathf.Max(0, character.CurrentStats.CurrentMana - chosen.ManaCost);
                        character.HPMPBarInstance?.SetMana(character.CurrentStats.CurrentMana, character.CurrentStats.MaxMana);
                    }
                }

            }
            else
            {
                // default attack
                var targetList = SelectTargets(character, AbilityTargetType.SingleEnemy);
                if (targetList.Count > 0)
                {
                    var target = targetList[0];

                    int simpleDamage = character.CurrentStats.baseDamage;

                    if (simpleDamage > 0)
                    {
                        target.TakeDamage(simpleDamage);

                        if (character.Team == superHero.Team)
                        {
                            HeroTeamRoundDamage += simpleDamage;
                        }
                        GrantMana(character, simpleDamage / 2);
                        character.PlayAttackAnimation();
                        Debug.Log($"{character.characterName} auto-attacks {target.characterName} for {simpleDamage}");
                    }
                }
            }

            yield return null;
        }

        private void GrantMana(BattleCharacter charWhoActed, int amount)
        {
            if (charWhoActed == null || amount <= 0) return;

            if (charWhoActed.CurrentStats != null)
            {

                var cur = charWhoActed.CurrentStats.CurrentMana;
                charWhoActed.CurrentStats.CurrentMana = Mathf.Min(cur + amount, charWhoActed.CurrentStats.MaxMana);

            }

            if (superHero != null)
            {
                if (superHero.Team == charWhoActed.Team)
                {
                    superHero.GainMana(amount);
                    gameUI.superHeroUI.UpdateManaUI();
                }
            }
        }
    }
}
