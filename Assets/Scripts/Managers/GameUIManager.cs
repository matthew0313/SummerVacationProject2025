using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour, ILocalPlayerSpawnCallback
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;

    [SerializeField] WardUI wardUI;
    [SerializeField] Slider hpSlider; // 체력 슬라이더
    Player player;

    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        player = localPlayer;
        player.hpComp.onHpChange += UpdateHpUI;
        UpdateHpUI();
    }

    void UpdateHpUI()
    {
        if (player == null || hpSlider == null) return;

        hpSlider.maxValue = player.hpComp.maxHp.Value;
        hpSlider.value = player.hpComp.hp.Value;
    }
}