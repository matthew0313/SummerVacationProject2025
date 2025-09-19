using NetworkAudioManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class GasPickup : Pickup
{
    [SerializeField] int gas = 10;
    [SerializeField] string pickupSound;
    [SerializeField] float pickupSoundDistMin = 5.0f, pickupSoundDistMax = 8.0f;
    [SerializeField] int despawnTime = 30;
    public readonly NetworkVariable<int> despawnCounter = new(30);
    private void OnEnable()
    {
        GameManager.Instance.onRoundEnd += OnRoundEnd;
    }
    private void OnDisable()
    {
        GameManager.Instance.onRoundEnd -= OnRoundEnd;
    }
    float counter = 0.0f;
    private void Update()
    {
        if (!HasAuthority) return;
        counter += Time.deltaTime;
        if(counter >= 1.0f)
        {
            counter -= 1.0f;
            despawnCounter.Value--;
            if (despawnCounter.Value <= 0) NetworkObject.Despawn();
        }
    }
    void OnRoundEnd()
    {
        if (HasAuthority) NetworkObject.Despawn();
    }
    protected override void OnPickup(Player picker)
    {
        NetworkAudioManager.Instance.PlaySound(pickupSound).SetDistance(transform.position, pickupSoundDistMin, pickupSoundDistMax);
        picker.inventory.AddGasRpc(gas);
        NetworkObject.Despawn();
    }
}