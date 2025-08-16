using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Pickup : NetworkBehaviour
{
    public float pickTimer = 1.0f;
    public Rigidbody2D rb { get; private set; }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (!HasAuthority) return;
        if (pickTimer > 0) pickTimer = Mathf.Max(pickTimer - Time.deltaTime, 0.0f);
    }
    [Rpc(SendTo.Authority)]
    public void PickupRpc(NetworkBehaviourReference pickerRef)
    {
        if (pickTimer <= 0.0f && pickerRef.TryGet(out Player picker)) OnPickup(picker);
    }
    protected abstract void OnPickup(Player picker);
}