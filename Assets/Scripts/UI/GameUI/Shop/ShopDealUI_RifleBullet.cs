using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_RifleBullet : ShopDealUI
{
    [SerializeField] int gasCost;
    [SerializeField] int ammoAdded;
    protected override void Purchase()
    {
        if (player.inventory.gas.Value < gasCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>가스가 부족합니다.</color>", Player.localPlayer);
            return;
        }
        player.inventory.gas.Value -= gasCost;
        player.inventory.rifleBullets += ammoAdded;
    }
}