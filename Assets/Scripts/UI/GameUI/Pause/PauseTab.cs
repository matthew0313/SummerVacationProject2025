using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PauseTab : Tab
{
    [SerializeField] TMP_InputField playerNameField;
    [SerializeField] TMP_Text sessionIDText;
    [SerializeField] Button exitButton;

    [Header("Tween")]
    [SerializeField] RectTransform tweenTarget;
    [SerializeField] CanvasGroup tweenAlphaTarget;
    [SerializeField] float tweenTime = 0.5f;
    Tween tween;
    float tweenCounter = 0.0f;
    public override void OpenTab()
    {
        base.OpenTab();
        if (tween == null) tween = DOTween.To(() => tweenCounter, value => tweenCounter = value, 1.0f, tweenTime)
                .SetAutoKill(false)
                .SetEase(Ease.OutCirc)
                .OnUpdate(() =>
                {
                    tweenTarget.anchorMin = new Vector2(0, 1.0f - tweenCounter);
                    tweenTarget.anchorMax = new Vector2(1.0f, 2.0f - tweenCounter);
                    tweenAlphaTarget.alpha = tweenCounter;
                });
        gameObject.SetActive(true);
        tween.OnStepComplete(null);
        tween.PlayForward();
    }
    public override void CloseTab()
    {
        base.CloseTab();
        tween.OnStepComplete(() => gameObject.SetActive(false));
        tween.PlayBackwards();
    }
    private void OnEnable()
    {
        playerNameField.onEndEdit.AddListener(OnPlayerNameFieldUpdate);
        exitButton.onClick.AddListener(ExitSession);
        SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
        OnPlayerNameChange(SessionManager.Instance.playerName);
        sessionIDText.text = SessionManager.Instance.activeSession.Id;
    }
    private void OnDisable()
    {
        playerNameField.onEndEdit.RemoveListener(OnPlayerNameFieldUpdate);
        exitButton.onClick.RemoveListener(ExitSession);
        SessionManager.Instance.onPlayerNameChange -= OnPlayerNameChange;
    }
    void OnPlayerNameFieldUpdate(string text) => SessionManager.Instance.SetPlayerName(text);
    void OnPlayerNameChange(string name) => playerNameField.text = name;
    void ExitSession() => SessionManager.Instance.LeaveSession();
}