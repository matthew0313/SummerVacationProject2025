using NetworkAudioManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class GasPickup : Pickup
{
    [SerializeField] int gas = 10;
    [SerializeField] string pickupSound;
    [SerializeField] float pickupSoundDistMin = 5.0f, pickupSoundDistMax = 8.0f;
    private void OnEnable()
    {
        GameManager.Instance.onRoundEnd += OnRoundEnd;
    }
    private void OnDisable()
    {
        GameManager.Instance.onRoundEnd -= OnRoundEnd;
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