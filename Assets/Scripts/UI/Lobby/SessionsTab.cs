using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using DG.Tweening;
using Unity.Services.Authentication;

public class SessionsTab : Tab
{
    [SerializeField] TMP_InputField sessionID;
    [SerializeField] Button joinOrStartSessionButton, startSessionButton;

    [Header("Session List")]
    [SerializeField] SessionListUI sessionList;
    [SerializeField] Button refreshButton;

    [Header("Player Name")]
    [SerializeField] TMP_InputField playerNameField;

    [Header("Tween")]
    [SerializeField] RectTransform tweenTarget;
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
        sessionID.onValueChanged.AddListener(OnIDValueChanged);
        joinOrStartSessionButton.onClick.AddListener(JoinOrStartSession);
        startSessionButton.onClick.AddListener(StartSession);
        refreshButton.onClick.AddListener(sessionList.Refresh);

        SessionManager.Instance.onSignIn += OnSignIn;
        if (SessionManager.Instance.signedIn) playerNameField.text = SessionManager.Instance.playerName;
        playerNameField.onEndEdit.AddListener(OnPlayerNameChange);
    }
    private void OnDisable()
    {
        sessionID.onValueChanged.RemoveListener(OnIDValueChanged);
        joinOrStartSessionButton.onClick.RemoveListener(JoinOrStartSession);
        startSessionButton.onClick.RemoveListener(StartSession);
        refreshButton.onClick.RemoveListener(sessionList.Refresh);

        SessionManager.Instance.onSignIn -= OnSignIn;
        playerNameField.onEndEdit.RemoveListener(OnPlayerNameChange);
    }
    void OnIDValueChanged(string value) => joinOrStartSessionButton.interactable = value != string.Empty;
    void JoinOrStartSession() => SessionManager.Instance.JoinOrStartSession(sessionID.text);
    void StartSession() => SessionManager.Instance.StartSession();
    void OnSignIn() => playerNameField.text = SessionManager.Instance.playerName;
    void OnPlayerNameChange(string text) => SessionManager.Instance.playerName = text;
}