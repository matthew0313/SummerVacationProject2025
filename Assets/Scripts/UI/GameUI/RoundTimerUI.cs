using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class RoundTimerUI : MonoBehaviour
{
    [SerializeField] TMP_Text timerText, roundInProgressText;
    private void Awake()
    {
        GameManager.Instance.onRoundInProgressChange += OnRoundInProgressChange;
        OnRoundInProgressChange(GameManager.Instance.roundInProgress);
    }
    private void Update()
    {
        int timeLeft = GameManager.Instance.timeLeft;
        timerText.text = $"{timeLeft/60}:{((timeLeft%60 >= 10) ? null : '0')}{timeLeft%60}";
    }
    void OnRoundInProgressChange(bool roundInProgress)
    {
        roundInProgressText.text =
            roundInProgress ? 
            "라운드 진행 중" :
            "라운드 시작 대기 중";
    }
}