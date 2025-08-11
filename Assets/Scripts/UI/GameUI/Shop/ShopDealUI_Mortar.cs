using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_Mortar : ShopDealUI
{
    [SerializeField] int gasCost;
    protected override void Purchase()
    {
        if (player.inventory.mortarUnlocked)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FFFF00>이미 해금되어 있습니다.</color>", Player.localPlayer);
            return;
        }
        if (player.inventory.gas < gasCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>가스가 부족합니다.</color>", Player.localPlayer);
            return;
        }
        player.inventory.gas -= gasCost;
        player.inventory.mortarUnlocked = true;
    }
    private void Update()
    {
        purchaseButton.interactable = !player.inventory.mortarUnlocked;
    }
}