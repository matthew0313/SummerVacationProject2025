using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;

    [SerializeField] WardUI m_wardUI;
    [SerializeField] RoundTimerUI m_roundTimerUI;
    public WardUI wardUI => m_wardUI;
    public RoundTimerUI roundTimerUI => m_roundTimerUI;
}