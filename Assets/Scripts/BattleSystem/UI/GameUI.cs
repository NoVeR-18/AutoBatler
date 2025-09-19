using BattleSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Control Buttons")]
    [SerializeField] private Button speedButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private TextMeshProUGUI totalDamagePerRound;
    [SerializeField] private Transform pauseImage;
    [SerializeField] private TextMeshProUGUI RoundText;

    public SuperHeroUI superHeroUI;

    public void AddListenerSpeed(UnityAction action)
    {
        if (speedButton != null)
            speedButton.onClick.AddListener(action);
    }

    public void AddListenerPause(UnityAction action)
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(action);
            pauseButton.onClick.AddListener(() =>
            {
                pauseImage.gameObject.SetActive(!pauseImage.gameObject.activeSelf);
            });
        }
    }

    public void AddListenerSurender(UnityAction action)
    {
        if (surrenderButton != null)
            surrenderButton.onClick.AddListener(action);
    }

    public void SetDamage(int value)
    {
        totalDamagePerRound.text = "Total damage \n " + value;
    }

    public void ReserUI()
    {
        totalDamagePerRound.text = "";
        superHeroUI.ResetUI();
        gameObject.SetActive(false);
    }

    internal void SetRound(int round)
    {
        if (RoundText != null)
            RoundText.text = "Round " + round;
    }
}
