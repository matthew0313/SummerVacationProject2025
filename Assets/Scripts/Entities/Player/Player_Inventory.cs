using System;
using Unity.Netcode;
using UnityEngine;
public class Player_Inventory : NetworkBehaviour
{
    Player origin;

    public readonly NetworkVariable<int> gas = new(0), bolt = new(0);
    [Rpc(SendTo.Authority)] public void AddGasRpc(int amount) => gas.Value += amount;
    [Rpc(SendTo.Authority)] public void AddBoltRpc(int amount) => bolt.Value += amount;

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

    [Header("Potion1")]
    [SerializeField] float potion1Heal = 30.0f;
    [SerializeField] float m_potion1Cooldown = 10.0f;

    [Header("Potion2")]
    [SerializeField] float potion2Duration = 5.0f;
    [SerializeField] float m_potion2Cooldown = 15.0f;
    public float potion1Cooldown => m_potion1Cooldown;
    public float potion2Cooldown => m_potion2Cooldown;
    public float potion1CooldownCounter { get; private set; } = 0.0f;
    public float potion2CooldownCounter { get; private set; } = 0.0f;

    [Header("Pickups")]
    [SerializeField] float pickupRadius;
    [SerializeField] LayerMask pickupLayer;

    [Header("Drops")]
    [SerializeField] float dropForce = 3.0f;
    [SerializeField] GasPickup gasPickupPrefab;
    [SerializeField] BoltPickup boltPickupPrefab;

    public event Action<Weapon> onWeapon2Change;

    public const int slotCount = 7;
    public int slotIndex { get; private set; } = 0;
    public readonly Slot[] slots = new Slot[slotCount];

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
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
            GameManager.Instance.onRoundStart += () => ResetInventory();
            GameManager.Instance.onRoundEnd += () => ResetInventory();
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
        if (potion1CooldownCounter > 0) potion1CooldownCounter = Mathf.Max(potion1CooldownCounter - Time.deltaTime, 0.0f);
        if (potion2CooldownCounter > 0) potion2CooldownCounter = Mathf.Max(potion2CooldownCounter - Time.deltaTime, 0.0f);
        foreach(var i in Physics2D.CircleCastAll(transform.position, pickupRadius, Vector2.up, 0.0f, pickupLayer))
        {
            GameObject hit = i.collider.attachedRigidbody != null ? i.collider.attachedRigidbody.gameObject : i.collider.gameObject;
            if(hit.TryGetComponent(out Pickup pickup))
            {
                pickup.PickupRpc(origin);
            }
        }
    }
    void ResetInventory()
    {
        gas.Value = 0; bolt.Value = 0; ward = 0; potion1 = 0; potion2 = 0;
        mortarUnlocked = false;
        ChangeSlot2WeaponRpc(-1);
    }
    void OnDeath()
    {
        slots[slotIndex].OnSlotUnequip();
        ward = 0; potion1 = 0; potion2 = 0;
        mortarUnlocked = false;
        for (int i = 0; i < gas.Value / 10 / 2; i++)
        {
            GasPickup tmp = Instantiate(gasPickupPrefab, transform.position, Quaternion.identity);
            float angle = UnityEngine.Random.Range(0, 360.0f);
            tmp.rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dropForce;
            tmp.NetworkObject.Spawn(true);
        }
        gas.Value = 0;
        for (int i = 0; i < bolt.Value / 10 / 2; i++)
        {
            BoltPickup tmp = Instantiate(boltPickupPrefab, transform.position, Quaternion.identity);
            float angle = UnityEngine.Random.Range(0, 360.0f);
            tmp.rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dropForce;
            tmp.NetworkObject.Spawn(true);
        }
        bolt.Value = 0;
    }
    void OnRespawn()
    {
        slots[slotIndex].OnSlotEquip();
        potion1CooldownCounter = 0.0f;
        potion2CooldownCounter = 0.0f;
    }
    public void ChangeSlot(int index)
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
            if (InputManager.Instance.ItemUseInput() && inventory.potion1 > 0 && inventory.potion1CooldownCounter <= 0.0f)
            {
                inventory.potion1--;
                inventory.origin.hpComp.hp.Value += inventory.potion1Heal;
                inventory.potion1CooldownCounter = inventory.potion1Cooldown;
            }
        }
    }
    public class Potion2Slot : Slot
    {
        public Potion2Slot(Player_Inventory inventory) : base(inventory) { }
        public override void OnSlotUpdate()
        {
            base.OnSlotUpdate();
            if (InputManager.Instance.ItemUseInput() && inventory.potion2 > 0 && inventory.potion2CooldownCounter <= 0.0f)
            {
                inventory.potion2--;
                inventory.origin.AttackBuff(inventory.potion2Duration);
                inventory.potion2CooldownCounter = inventory.potion2Cooldown;
            }
        }
    }
}
