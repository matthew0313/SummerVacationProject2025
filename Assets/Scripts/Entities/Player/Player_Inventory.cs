using System;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class Player_Inventory : NetworkBehaviour
{
    Player origin;

    [SerializeField] Weapon weapon1, weapon2;
    int m_gas = 0, m_bolt = 0, m_ward = 0, m_potion1 = 0, m_potion2 = 0;
    public int gas
    {
        get => m_gas;
        set { m_gas = value; onGasChange?.Invoke(value); }
    }
    public int bolt
    {
        get => m_bolt;
        set { m_bolt = value; onBoltChange?.Invoke(value); }
    }
    public int ward
    {
        get => m_ward;
        set { m_ward = value; onWardChange?.Invoke(value); }
    }
    public int potion1
    {
        get => m_potion1;
        set {  m_potion1 = value; onPotion1Change?.Invoke(value);}
    }
    public int potion2
    {
        get => m_potion2;
        set { m_potion2 = value; onPotion2Change?.Invoke(value); }
    }

    public event Action<int> onGasChange, onBoltChange, onWardChange, onPotion1Change, onPotion2Change;
    private void Awake()
    {
        origin = GetComponent<Player>();
    }
    private void Start()
    {
        if (HasAuthority)
        {
            GameManager.Instance.onRoundStart += OnRoundStart;
            GameManager.Instance.onRoundEnd += OnRoundEnd;
        }
    }
    void OnRoundStart()
    {

    }
    void OnRoundEnd()
    {

    }

    [Rpc(SendTo.Authority)]
    public void SetGasRpc(int gas) => this.gas.Value = gas;
    [Rpc(SendTo.Authority)]
    public void SetBoltsRpc(int bolts) => this.bolts.Value = bolts;
    [Rpc(SendTo.Authority)]
    public void AddGasRpc(int amount) => this.gas.Value += amount;
}
