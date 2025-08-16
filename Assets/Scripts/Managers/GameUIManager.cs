using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : TabOpener
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;

    [SerializeField] Animator anim;

    [Header("Tabs")]
    [SerializeField] ShopTab m_shopTab;
    public ShopTab shopTab => m_shopTab;

    [Header("Hp")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] Transform hpBar;

    [Header("Other UIs")]
    [SerializeField] RoundTimerUI m_roundTimerUI;
    [SerializeField] MessageUI m_messageUI;
    [SerializeField] InventoryUI m_inventoryUI;
    public RoundTimerUI roundTimerUI => m_roundTimerUI;
    public MessageUI messageUI => m_messageUI;
    public InventoryUI inventoryUI => m_inventoryUI;

    readonly int tiedID = Animator.StringToHash("Tied");
    readonly int wonID = Animator.StringToHash("Won");
    readonly int lostID = Animator.StringToHash("Lost");
    
    private void Awake()
    {
        GameManager.Instance.onRoundComplete += result =>
        {
            if (result == RoundResult.Tied) anim.SetTrigger(tiedID);
            else if (result == RoundResult.Won) anim.SetTrigger(wonID);
            else if(result == RoundResult.Lost) anim.SetTrigger(lostID);
        };
    }
    private void Update()
    {
        if (InputManager.Instance.EscapeInput()) CloseTab();
        hpText.text = $"{Player.localPlayer.hpComp.hp.Value} | {Player.localPlayer.hpComp.maxHp.Value}";
        hpBar.localScale = new Vector3(Player.localPlayer.hpComp.hp.Value / Player.localPlayer.hpComp.maxHp.Value, 1.0f, 1.0f);
    }
}