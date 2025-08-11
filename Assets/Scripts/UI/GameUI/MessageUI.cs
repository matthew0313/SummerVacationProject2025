using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MessageUI : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text messageTextPrefab;
    [SerializeField] Transform messageTextAnchor;

    readonly List<TMP_Text> messageTexts = new();

    private void Awake()
    {
        inputField.onSubmit.AddListener(OnSubmit);
        inputField.onSelect.AddListener(text => InputManager.Instance.inputEnabled = false);
        inputField.onDeselect.AddListener(text => InputManager.Instance.inputEnabled = true);
    }
    private void OnEnable()
    {
        MessageManager.Instance.onMessageAdd += OnMessageAdd;
    }

    private void OnDisable()
    {
        MessageManager.Instance.onMessageAdd -= OnMessageAdd;
    }
    void OnSubmit(string msg)
    {
        inputField.text = string.Empty;
        EventSystem.current.SetSelectedGameObject(null);
        if (msg != string.Empty)
        {
            if (msg.StartsWith("/t "))
            {
                msg = msg.Substring(3);
                if(Player.localPlayer.team.Value == Team.None)
                {
                    MessageManager.Instance.SendMessageRpc("<color=#FF0000>현재 팀에 속해 있지 않습니다.</color>");
                    return;
                }
                foreach (var player in GameManager.Instance.players)
                {
                    if (player.team.Value != Team.None && player.team.Value == Player.localPlayer.team.Value)
                    {
                        MessageManager.Instance.SendMessageRpc($"<b>[팀 채팅] {SessionManager.Instance.playerName}</b>: {msg}");
                    }
                }
            }
            else
            {
                MessageManager.Instance.SendMessageRpc($"<b>{SessionManager.Instance.playerName}</b>: {msg}");
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
    private void OnMessageAdd(string message)
    {
        TMP_Text tmp;
        if (messageTexts.Count >= MessageManager.Instance.maxMessages)
        {
            tmp = messageTexts[0]; messageTexts.RemoveAt(0);
            tmp.transform.SetAsLastSibling();
        }
        else tmp = Instantiate(messageTextPrefab, messageTextAnchor);
        tmp.text = message.ToString();
        messageTexts.Add(tmp);
        tmp.gameObject.SetActive(true);
    }
}