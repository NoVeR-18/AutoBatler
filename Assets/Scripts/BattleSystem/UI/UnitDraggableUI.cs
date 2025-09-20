using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSystem
{
    public class UnitDraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        public Image portrait;
        [HideInInspector]
        public BattleCharacter character;

        private Transform originalParent;
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        private BattleSpawnPoint assignedPoint;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            transform.SetParent(canvas.transform); // тянем поверх UI
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            // пробуем найти спавнпоинт в мире
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                var spawnPoint = hit.GetComponent<BattleSpawnPoint>();
                if (spawnPoint != null && spawnPoint.team == BattleTeam.Alias)
                {
                    AssignToSpawnPoint(spawnPoint);
                    return;
                }
            }

            // если не попали — возвращаем назад
            ResetToOriginal();
        }
        public void Setup(BattleCharacter unit)
        {
            character = unit;

            // если у тебя в BattleCharacter есть спрайт
            if (portrait != null && unit.Portrait != null)
                portrait.sprite = unit.Portrait;
        }

        // чтобы можно было взять юнита при драг-н-дропе
        public BattleCharacter GetCharacter()
        {
            return character;
        }
        private void AssignToSpawnPoint(BattleSpawnPoint point)
        {

            assignedPoint = point;
            var unit = Instantiate(character, point.transform);
            unit.transform.localPosition = Vector3.zero;
            point.AssignUnitDirect(unit);

            // иконку можно оставить в UI снизу, а можно визуально закрепить над точкой
            transform.position = Camera.main.WorldToScreenPoint(point.transform.position);
            Destroy(gameObject);
        }

        private void ResetToOriginal()
        {
            assignedPoint = null;
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }

        public BattleSpawnPoint GetAssignedPoint() => assignedPoint;

    }
}
