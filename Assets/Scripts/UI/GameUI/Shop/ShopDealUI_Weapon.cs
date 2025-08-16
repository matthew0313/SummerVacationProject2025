using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_Weapon : ShopDealUI
{
    [SerializeField] int gasCost;
    [SerializeField] int weaponIndex;
    protected override void Purchase()
    {
        if (player.inventory.slot2WeaponIndex == weaponIndex)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FFFF00>이미 소지 중인 무기입니다.</color>", Player.localPlayer);
            return;
        }
        if (player.inventory.gas.Value < gasCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>가스가 부족합니다.</color>", Player.localPlayer);
            return;
        }
        player.inventory.gas.Value -= gasCost;
        player.inventory.ChangeSlot2WeaponRpc(weaponIndex);
    }
    private void Update()
    {
        purchaseButton.interactable = player.inventory.slot2WeaponIndex != weaponIndex;
    }
}