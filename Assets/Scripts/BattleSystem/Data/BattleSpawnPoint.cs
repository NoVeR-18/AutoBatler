using UnityEngine;

namespace BattleSystem
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BattleSpawnPoint : MonoBehaviour
    {
        public BoxCollider2D boxCollider;
        public SpriteRenderer spriteRenderer;
        public BattleTeam team;
        public int index;

        public void AssignUnitDirect(BattleCharacter character)
        {
            assignedCharacter = character;
            if (boxCollider != null)
                boxCollider.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
        }

        private BattleCharacter assignedCharacter;

        public bool HasDirectAssignment() => assignedCharacter != null;
        public BattleCharacter GetAssignedCharacter() => assignedCharacter;
    }
}
