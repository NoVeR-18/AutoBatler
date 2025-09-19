using UnityEngine;

namespace BattleSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class BattleSpawnPoint : MonoBehaviour
    {
        public BattleTeam team;
        public int index;

        private UnitDraggableUI assignedUnit;

        public void AssignUnit(UnitDraggableUI unit)
        {
            assignedUnit = unit;
        }

        public void ClearAssignedUnit()
        {
            assignedUnit = null;
        }
        public void AssignUnitDirect(BattleCharacter character)
        {
            assignedCharacter = character;
        }

        private BattleCharacter assignedCharacter;

        public bool HasDirectAssignment() => assignedCharacter != null;
        public BattleCharacter GetAssignedCharacter() => assignedCharacter;
        public UnitDraggableUI GetAssignedUnit() => assignedUnit;
    }
}
