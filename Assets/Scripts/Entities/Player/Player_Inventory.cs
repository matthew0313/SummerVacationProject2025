using System;
using System.Linq;
using TMPro.EditorUtilities;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.Rendering;
using UnityEngine;
public class Player_Inventory : NetworkBehaviour
{
    Player origin;

    public int gas = 0, bolt = 0;
    [Rpc(SendTo.Authority)] public void AddGasRpc(int amount) => gas += amount;
    [Rpc(SendTo.Authority)] public void AddBoltRpc(int amount) => bolt += amount;

    [Header("Weapons")]
    [SerializeField] Spear knifePrefab;
    [SerializeField] Mortar mortarPrefab;
    [SerializeField] Weapon[] slot2WeaponPrefabs;
    Weapon[] slot2Weapons;

    [Header("Others")]
    [SerializeField] int m_maxWards = 3;
    [SerializeField] int m_maxPotion1 = 5, m_maxPotion2 = 5;
    public int maxWards => m_maxWards;
    public int maxPotion1 => m_maxPotion1;
    public int maxPotion2 => m_maxPotion2;
    public int ward = 0, potion1 = 0, potion2 = 0;
    public bool mortarUnlocked = false;
    public int pistolBullets = 0, rifleBullets = 0;

    public event Action<Weapon> onWeapon2Change;

    public const int slotCount = 7;
    public int slotIndex { get; private set; } = 0;
    public readonly Slot[] slots = new Slot[slotCount];
    private void Awake()
    {
        origin = GetComponent<Player>();
        slots[0] = new WeaponSlot(this);
        slots[1] = new WeaponSlot(this);
        slots[2] = new WardSlot(this);
        slots[3] = new Potion1Slot(this);
        slots[4] = new Potion2Slot(this);
        slots[5] = new WeaponSlot(this);
        slots[6] = new Slot(this);
    }
    private void Start()
    {
        if (HasAuthority)
        {
            GameManager.Instance.onRoundStart += ResetInventory;
            GameManager.Instance.onRoundEnd += ResetInventory;
            origin.hpComp.onDeath += OnDeath;
            origin.hpComp.onRespawn += OnRespawn;

            Spear knife = Instantiate(knifePrefab);
            knife.NetworkObject.Spawn();
            knife.active.Value = false;
            (slots[0] as WeaponSlot).SetWeapon(knife);
            Mortar mortar = Instantiate(mortarPrefab);
            mortar.NetworkObject.Spawn();
            mortar.active.Value = false;
            (slots[5] as WeaponSlot).SetWeapon(mortar);
            slot2Weapons = new Weapon[slot2WeaponPrefabs.Length];
            for(int i = 0; i < slot2Weapons.Length; i++)
            {
                Weapon tmp = Instantiate(slot2WeaponPrefabs[i]);
                tmp.NetworkObject.Spawn(true);
                tmp.active.Value = false;
                slot2Weapons[i] = tmp;
            }
            slots[slotIndex].OnSlotEquip();
        }
    }
    bool isDead => origin.hpComp.isDead.Value;
    private void Update()
    {
        if (!HasAuthority || isDead) return;
        slots[slotIndex].OnSlotUpdate();
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChangeSlot(i); break;
            }
        }
    }
    void ResetInventory()
    {
        gas = 0; bolt = 0; ward = 0; potion1 = 0; potion2 = 0;
        mortarUnlocked = false;
        ChangeSlot2WeaponRpc(-1);
    }
    void OnDeath()
    {
        slots[slotIndex].OnSlotUnequip();
    }
    void OnRespawn()
    {
        slots[slotIndex].OnSlotEquip();
    }
    void ChangeSlot(int index)
    {
        if (slotIndex == index || index == 5 && !mortarUnlocked) return;
        slots[slotIndex].OnSlotUnequip();
        slotIndex = index;
        slots[slotIndex].OnSlotEquip();
    }
    public int slot2WeaponIndex { get; private set; } = -1;
    [Rpc(SendTo.Owner)]
    public void ChangeSlot2WeaponRpc(int index)
    {
        if(index < -1 || index >= slot2Weapons.Length) return;
        WeaponSlot slot = (slots[1] as WeaponSlot);
        if (index == -1) slot.SetWeapon(null);
        else slot.SetWeapon(slot2Weapons[index]);
        slot2WeaponIndex = index;
    }
    public class Slot
    {
        public readonly Player_Inventory inventory;
        public Slot(Player_Inventory inventory)
        {
            this.inventory = inventory;
        }
        public virtual void OnSlotEquip() { }
        public virtual void OnSlotUpdate() { }
        public virtual void OnSlotUnequip() { }
    }
    public class WeaponSlot : Slot
    {
        public WeaponSlot(Player_Inventory inventory) : base(inventory) { }
        public Weapon weapon { get; private set; }
        public event Action<Weapon> onWeaponChange;
        public void SetWeapon(Weapon weapon)
        {
            this.weapon = weapon;
            if (weapon != null) weapon.owner = inventory.origin;
            onWeaponChange?.Invoke(weapon);
        }
        public override void OnSlotEquip()
        {
            base.OnSlotEquip();
            onWeaponChange += EquipWeapon;
            if (weapon != null) EquipWeapon(weapon);
        }
        public override void OnSlotUnequip()
        {
            base.OnSlotUnequip();
            onWeaponChange -= EquipWeapon;
            if (equipped != null) EquipWeapon(null);
        }
        Weapon equipped = null;
        void EquipWeapon(Weapon weapon)
        {
            if (equipped != null) equipped.OnUnequipped();
            equipped = weapon;
            if(equipped != null) equipped.OnEquipped();
        }
    }
    public class WardSlot : Slot
    {
        public WardSlot(Player_Inventory inventory) : base(inventory) { }
        public override void OnSlotEquip()
        {
            base.OnSlotEquip();
            inventory.origin.wardComp.wardPlaceMode = true;
        }
        public override void OnSlotUnequip()
        {
            base.OnSlotUnequip();
            inventory.origin.wardComp.wardPlaceMode = false;
        }
    }
    public class Potion1Slot : Slot
    {
        public Potion1Slot(Player_Inventory inventory) : base(inventory) { }
        public override void OnSlotUpdate()
        {
            base.OnSlotUpdate();
            if (Input.GetMouseButtonDown(0))
            {

            }
        }
    }
    public class Potion2Slot : Slot
    {
        public Potion2Slot(Player_Inventory inventory) : base(inventory) { }
        public override void OnSlotUpdate()
        {
            base.OnSlotUpdate();
            if (Input.GetMouseButtonDown(0))
            {

            }
        }
    }
}
