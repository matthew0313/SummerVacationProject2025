using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : TabOpener
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;


    [Header("Other UIs")]
    [SerializeField] WardUI m_wardUI;
    [SerializeField] RoundTimerUI m_roundTimerUI;
    [SerializeField] MessageUI m_messageUI;
    public WardUI wardUI => m_wardUI;
    public RoundTimerUI roundTimerUI => m_roundTimerUI;
    public MessageUI messageUI => m_messageUI;
}