using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : TabOpener
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;

    [Header("Tabs")]
    [SerializeField] ShopTab m_shopTab;
    public ShopTab shopTab => m_shopTab;

    [Header("Other UIs")]
    [SerializeField] RoundTimerUI m_roundTimerUI;
    [SerializeField] MessageUI m_messageUI;
    [SerializeField] InventoryUI m_inventoryUI;
    public RoundTimerUI roundTimerUI => m_roundTimerUI;
    public MessageUI messageUI => m_messageUI;
    public InventoryUI inventoryUI => m_inventoryUI;
    private void Update()
    {
        if (InputManager.Instance.EscapeInput()) CloseTab();
    }
}