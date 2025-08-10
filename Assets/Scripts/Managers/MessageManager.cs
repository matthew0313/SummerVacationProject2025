using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MessageManager : NetworkBehaviour
{
    public static MessageManager Instance { get; private set; }
    public MessageManager() => Instance = this;


    [SerializeField] int m_maxMessages = 50;
    public int maxMessages => m_maxMessages;

    public readonly List<string> messages = new();
    public event Action<string> onMessageAdd;


    public void SendMessageToAll(string message) => SendMessageRpc(message);
    public void SendMessageToPlayer(string message, Player player) => SendMessageRpc(message, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));

    [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
    public void SendMessageRpc(FixedString512Bytes message, RpcParams rpcParams = new())
    {
        string msg = message.ToString();
        messages.Add(msg);
        if(messages.Count > maxMessages) messages.RemoveAt(0);
        onMessageAdd?.Invoke(msg);
    }
}