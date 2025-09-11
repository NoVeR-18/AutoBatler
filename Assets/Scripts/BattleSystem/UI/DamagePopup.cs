using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BattleSystem
{
    public class DamagePopup : MonoBehaviour
    {
        public TMP_Text damageText;

        public void Setup(int amount)
        {
            damageText.text = amount.ToString();
            transform.DOMoveY(transform.position.y + 1, 0.5f).SetEase(Ease.OutCubic);
            damageText.DOFade(0, 0.5f).SetDelay(0.3f).OnComplete(() =>
            {
                DOTween.Kill(gameObject);
                Destroy(gameObject);
            });
        }
        public void Setup(int amount, Color color)
        {
            damageText.text = amount.ToString();
            damageText.color = color;
            transform.DOMoveY(transform.position.y + 1, 0.5f).SetEase(Ease.OutCubic);
            damageText.DOFade(0, 0.5f).SetDelay(0.3f).OnComplete(() =>
            {
                DOTween.Kill(gameObject);
                Destroy(gameObject);
            });
        }
    }
}