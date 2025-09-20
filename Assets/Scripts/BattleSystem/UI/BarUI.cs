using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class BarUI : MonoBehaviour
    {
        [SerializeField] private Image HPFillImage;
        [SerializeField] private Image ManaFillImage;

        public void SetHealth(float current, float max)
        {
            if (HPFillImage == null) return;
            HPFillImage.fillAmount = current / max;
        }
        public void SetMana(float current, float max)
        {
            if (ManaFillImage == null) return;
            ManaFillImage.fillAmount = current / max;
        }
        public void SetColor(Color color)
        {
            HPFillImage.color = color;
        }
    }
}