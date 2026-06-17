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
            if (msg.StartsWith("/"))
            {
                if (msg.StartsWith("/t "))
                {
                    if (Player.localPlayer.team.Value == Team.None)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>현재 팀에 속해 있지 않습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(3);
                    if(msg != string.Empty)
                    {
                        foreach (var player in GameManager.Instance.players)
                        {
                            if (player.team.Value != Team.None && player.team.Value == Player.localPlayer.team.Value)
                            {
                                MessageManager.Instance.SendMessageToPlayer($"<b>[팀 채팅] {SessionManager.Instance.playerName}</b>: {msg}", player);
                            }
                        }
                    }
                }
                else if (msg.StartsWith("/timer "))
                {
                    if(NetworkManager.Singleton.LocalClientId != GameManager.Instance.cheatOwner.Value)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>치트 오너만 이 명령어를 사용할 수 있습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(7);
                    if (msg == "pause")
                    {
                        GameManager.Instance.ChangeTimerPausedRpc(true); return;
                    }
                    if(msg == "resume")
                    {
                        GameManager.Instance.ChangeTimerPausedRpc(false); return;
                    }
                    if(msg == "skip")
                    {
                        GameManager.Instance.ChangeTimeLeftRpc(1); return;
                    }
                    if (msg.StartsWith("add "))
                    {
                        msg = msg.Substring(4);
                        if(int.TryParse(msg, out int val) && val > 0)
                        {
                            GameManager.Instance.timeLeft.Value += val; return;
                        }
                        else
                        {
                            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>유효한 숫자가 아닙니다.</color>", Player.localPlayer); return;
                        }
                    }
                }
                else if(msg.StartsWith("/transferOwner "))
                {
                    if (NetworkManager.Singleton.LocalClientId != GameManager.Instance.cheatOwner.Value)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>치트 오너만 이 명령어를 사용할 수 있습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(15);
                    foreach (var i in GameManager.Instance.players)
                    {
                        if (i.playerName.Value == msg)
                        {
                            GameManager.Instance.ChangeCheatOwnerRpc(i.OwnerClientId); return;
                        }
                    }
                    MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>해당 플레이어를 찾지 못했습니다.</color>", Player.localPlayer);
                    return;
                }
                else if (msg.StartsWith("/setTeam "))
                {
                    if (NetworkManager.Singleton.LocalClientId != GameManager.Instance.cheatOwner.Value)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>치트 오너만 이 명령어를 사용할 수 있습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(9);
                    string playerName = msg.Substring(0, msg.IndexOf(' '));
                    string team = msg.Substring(msg.IndexOf(' ') + 1);
                    foreach (var i in GameManager.Instance.players)
                    {
                        if (i.playerName.Value == playerName)
                        {
                            if (i.inRound.Value)
                            {
                                MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>라운드 진행 중인 플레이어의 팀을 바꿀 수 없습니다.</color>", Player.localPlayer);
                                return;
                            }
                            switch (team)
                            {
                                case "red": i.SetTeamRpc(Team.Red); break;
                                case "blue": i.SetTeamRpc(Team.Blue); break;
                                case "none": i.SetTeamRpc(Team.None); break;
                                default:
                                    MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>유효한 팀이 아닙니다. (blue/red/none)</color>", Player.localPlayer);
                                    return;
                            }
                            return;
                        }
                    }
                    MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>해당 플레이어를 찾지 못했습니다.</color>", Player.localPlayer);
                    return;
                }
                else if(msg.StartsWith("/spectator "))
                {
                    if (Player.localPlayer.inRound.Value)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>라운드 진행 중 관전자로 전환할 수 없습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(11);
                    switch (msg)
                    {
                        case "true": Player.localPlayer.spectator.Value = true; break;
                        case "false": Player.localPlayer.spectator.Value = false; break;
                        default: MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>유효한 값이 아닙니다. (true/false)", Player.localPlayer); return;
                    }
                    return;
                }
                else if(msg.StartsWith("/forcedJoin "))
                {
                    if (NetworkManager.Singleton.LocalClientId != GameManager.Instance.cheatOwner.Value)
                    {
                        MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>치트 오너만 이 명령어를 사용할 수 있습니다.</color>", Player.localPlayer);
                        return;
                    }
                    msg = msg.Substring(12);
                    foreach (var i in GameManager.Instance.players)
                    {
                        if (i.playerName.Value == msg && !i.inRound.Value)
                        {
                            GameManager.Instance.ForcedJoinRpc(i.OwnerClientId);
                            return;
                        }
                    }
                    MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>해당 플레이어를 찾지 못했습니다.</color>", Player.localPlayer);
                    return;

                }
                MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>유효한 명령어가 아닙니다.</color>", Player.localPlayer);
            }
            else
            {
                MessageManager.Instance.SendMessageToAll($"<b>{SessionManager.Instance.playerName}</b>: {msg}");
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