using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class RoundTimerUI : MonoBehaviour
{
    [SerializeField] TMP_Text timerText, roundInProgressText;
    private void Start()
    {
        GameManager.Instance.roundInProgress.OnValueChanged += OnRoundInProgressChange;
        OnRoundInProgressChange(false, GameManager.Instance.roundInProgress.Value);
        GameManager.Instance.timeLeft.OnValueChanged += OnTimeLeftChange;
        OnTimeLeftChange(0, GameManager.Instance.timeLeft.Value);
    }
    void OnRoundInProgressChange(bool prev, bool roundInProgress)
    {
        roundInProgressText.text =
            roundInProgress ? 
            "라운드 진행 중" :
            "라운드 시작 대기 중";
    }
    void OnTimeLeftChange(int prev, int timeLeft)
    {
        timerText.text = $"{timeLeft / 60}:{((timeLeft % 60 >= 10) ? null : '0')}{timeLeft % 60}";
    }
}