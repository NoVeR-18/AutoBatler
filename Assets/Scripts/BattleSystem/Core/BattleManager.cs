using DG.Tweening;
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

        public List<BattleCharacter> teamA = new();
        public List<BattleCharacter> teamB = new();

        [Header("Timing")]
        public float actionInterval = 1f;

        [Header("SuperHero")]
        public SuperHero superHero;


        [Header("UI")]
        public SuperHeroUI superHeroUI;

        private Dictionary<BattleTeam, List<BattleSpawnPoint>> _spawnPoints;
        private Dictionary<BattleSpawnPoint, BattleCharacter> occupiedSpawns = new();

        private List<BattleCharacter> turnOrder = new();

        private void Start()
        {
            _spawnPoints = spawnPoints.GetSpawnPoints();
            StartBattle();

        }
        public void SetTeams(List<BattleCharacter> teamAList, List<BattleCharacter> teamBList)
        {
            teamA = teamAList;
            teamB = teamBList;
        }

        public void StartBattle()
        {
            if (superHeroUI != null)
                superHero.gameObject.SetActive(true);
            occupiedSpawns.Clear();
            SpawnTeams();
            BuildTurnOrder();
            StopAllCoroutines();
            StartCoroutine(TurnLoop());
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
                occupiedSpawns[spawn] = character;
                teamA[i] = character;
            }

            // team B
            for (int i = 0; i < teamB.Count && i < _spawnPoints[BattleTeam.Team2].Count; i++)
            {
                var spawn = _spawnPoints[BattleTeam.Team2][i];
                var character = Instantiate(teamB[i], spawn.transform.position, Quaternion.identity);
                character.Team = BattleTeam.Team2;
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
                if (superHeroUI != null)
                    superHeroUI.Setup(superHero);
            }
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
            return new List<BattleCharacter> { list[Random.Range(0, list.Count)] };
        }

        private void ApplyAbility(BattleCharacter caster, BattleCharacter target, AbilityData ability)
        {
            if (target == null || !target.IsAlive) return;

            if (ability == null) return;

            int totalDamage = ability.GetDamage();

            if (totalDamage > 0)
            {
                target.TakeDamage(totalDamage);

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
            while (!IsBattleOver())
            {
                if (turnOrder.Count == 0)
                {
                    BuildTurnOrder();
                    yield return null;
                    continue;
                }

                for (int i = 0; i < turnOrder.Count; i++)
                {
                    var current = turnOrder[i];

                    if (current == null || !current.IsAlive || !current.CanAct())
                    {
                        yield return new WaitForSeconds(actionInterval);
                        continue;
                    }

                    yield return StartCoroutine(ExecuteActionCoroutine(current));
                    CleanDeadCharactersAndFreeSpawns();
                    yield return new WaitForSeconds(actionInterval);
                }

                // end of round
                UpdateAllCooldowns(actionInterval);
            }

            Debug.Log("Battle Over!");
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
                character.StartCooldown(chosen);

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
                // don`t have any ability to use — try to use basic ability (ManaCost == 0 and off cooldown)
                AbilityData spell = character.Abilities.FirstOrDefault(a =>
                    a.ManaCost == 0 && character.GetCooldowns().TryGetValue(a, out float rem) && rem <= 0f);

                if (spell != null)
                {
                    var targets = SelectTargets(character, spell.TargetType);

                    foreach (var t in targets)
                        ApplyAbility(character, t, spell);

                    character.PlayAttackAnimation(spell.animationTrigger);
                    character.StartCooldown(spell);

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
                            GrantMana(character, simpleDamage / 2);
                            character.PlayAttackAnimation();
                            Debug.Log($"{character.characterName} auto-attacks {target.characterName} for {simpleDamage}");
                        }
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
                    superHeroUI.UpdateManaUI();
                }
            }
        }

        private void UpdateAllCooldowns(float deltaTime)
        {
            foreach (var character in teamA.Concat(teamB))
            {
                character.TickCooldowns(deltaTime);
            }
            if (superHero != null)
            {
                superHero.TickCooldowns(deltaTime);
                superHeroUI.UpdateCooldowns(superHero);
            }

        }
    }
}
