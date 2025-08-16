using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class RoundTimerUI : MonoBehaviour
{
    [SerializeField] TMP_Text timerText, roundInProgressText;
    private void Start()
    {
        GameManager.Instance.gameState.OnValueChanged += OnGameStateChange;
        OnGameStateChange(GameState.Intermission, GameManager.Instance.gameState.Value);
        GameManager.Instance.timeLeft.OnValueChanged += OnTimeLeftChange;
        OnTimeLeftChange(0, GameManager.Instance.timeLeft.Value);
    }
    void OnGameStateChange(GameState prev, GameState gameState)
    {
        if(gameState == GameState.Intermission)
        {
            roundInProgressText.text = "라운드 시작 대기 중";
        }
        if(gameState == GameState.RoundInProgress)
        {
            roundInProgressText.text = "라운드 진행 중";
        }
        if(gameState == GameState.RoundCompleted)
        {
            roundInProgressText.text = "라운드 종료";
        }
    }
    void OnTimeLeftChange(int prev, int timeLeft)
    {
        timerText.text = $"{timeLeft / 60}:{((timeLeft % 60 >= 10) ? null : '0')}{timeLeft % 60}";
    }
}