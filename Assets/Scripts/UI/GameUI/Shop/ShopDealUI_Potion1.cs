using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_Potion1 : ShopDealUI
{
    [SerializeField] int gasCost;
    [SerializeField] int potionAdded;
    protected override void Purchase()
    {
        if (player.inventory.potion1 + potionAdded > player.inventory.maxPotion1)
        {
            MessageManager.Instance.SendMessageToPlayer($"<color=#FF0000>해당 아이템의 소지 상한을 넘길 수 없습니다. ({player.inventory.maxPotion1})", Player.localPlayer);
            return;
        }
        if (player.inventory.gas.Value < gasCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>가스가 부족합니다.</color>", Player.localPlayer);
            return;
        }
        player.inventory.gas.Value -= gasCost;
        player.inventory.potion1 += potionAdded;
    }
}