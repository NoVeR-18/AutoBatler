using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleSystem
{
    public class BattleSpawnData : MonoBehaviour
    {
        [Header("Team A (Player) Spawn Points")]
        public List<BattleSpawnPoint> teamASpawnPoints;

        [Header("Team B (Enemy) Spawn Points")]
        public List<BattleSpawnPoint> teamBSpawnPoints;

        [Header("Super Hero Spawn Point")]
        public BattleSpawnPoint superHeroSpawn;

        public Dictionary<BattleTeam, List<BattleSpawnPoint>> GetSpawnPoints()
        {
            var dict = new Dictionary<BattleTeam, List<BattleSpawnPoint>>
            {
                { BattleTeam.Team1, teamASpawnPoints.OrderBy(sp => sp.index).ToList() },
                { BattleTeam.Team2, teamBSpawnPoints.OrderBy(sp => sp.index).ToList() }
            };

            return dict;
        }
    }
}
