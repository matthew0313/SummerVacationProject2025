using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InventoryUI : MonoBehaviour, ILocalPlayerSpawnCallback
{
    [SerializeField] SlotUI[] slotUIs = new SlotUI[Player_Inventory.slotCount];
    [SerializeField] Image weapon2Image, bulletsImage;
    [SerializeField] TMP_Text gas, bolt, ward, potion1, potion2, bullets;

    int selectedIndex = 0;
    SlotUI selected;

    public void OnLocalPlayerSpawn(Player player)
    {
        var weapon2Slot = (player.inventory.slots[1] as Player_Inventory.WeaponSlot);
        weapon2Slot.onWeaponChange += OnWeapon2Change;
        OnWeapon2Change(weapon2Slot.weapon);
    }

    private void OnWeapon2Change(Weapon weapon)
    {
        if (weapon != null)
        {
            weapon2Image.sprite = weapon.weaponSprite;
            weapon2Image.gameObject.SetActive(true);
        }
        else weapon2Image.gameObject.SetActive(false);
    }

    private void Awake()
    {
        selected = slotUIs[0];
        selected.SetSelected(true);
    }
    private void Update()
    {
        if (Player.localPlayer == null) return;
        if(selectedIndex != Player.localPlayer.inventory.slotIndex)
        {
            selected.SetSelected(false);
            selectedIndex = Player.localPlayer.inventory.slotIndex;
            selected = slotUIs[selectedIndex];
            selected.SetSelected(true);
        }
        gas.text = Player.localPlayer.inventory.gas.ToString();
        bolt.text = Player.localPlayer.inventory.bolt.ToString();
        ward.text = Player.localPlayer.inventory.ward.ToString();
        potion1.text = Player.localPlayer.inventory.potion1.ToString();
        potion2.text = Player.localPlayer.inventory.potion2.ToString();
        var weapon2Slot = Player.localPlayer.inventory.slots[1] as Player_Inventory.WeaponSlot;
        if(weapon2Slot.weapon is Gun gun)
        {
            bulletsImage.sprite = gun.bulletSprite;
            bulletsImage.gameObject.SetActive(true);
            bullets.text = gun.bullets.ToString();
        }
        else
        {
            bulletsImage.gameObject.SetActive(false);
            bullets.text = "0";
        }
    }
}