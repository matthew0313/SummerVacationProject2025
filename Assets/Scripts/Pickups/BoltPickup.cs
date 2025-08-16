using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class BoltPickup : Pickup
{
    [SerializeField] int bolt = 10;
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
        picker.inventory.AddBoltRpc(bolt);
        NetworkObject.Despawn();
    }
}