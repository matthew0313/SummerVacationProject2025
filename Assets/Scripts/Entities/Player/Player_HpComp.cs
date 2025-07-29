using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player_HpComp : HpComp
{
    Player origin;
    public override Entity entity => origin;

    [SerializeField] float respawnTime = 3.0f;
    public Transform respawnPoint;
    protected override void Awake()
    {
        base.Awake();
        origin = GetComponent<Player>();
    }
    protected override void OnDeath()
    {
        base.OnDeath();
        StartCoroutine(Respawning());
    }
    IEnumerator Respawning()
    {
        yield return new WaitForSeconds(respawnTime);
        isDead.Value = false;
        hp.Value = maxHp.Value;
        origin.TeleportRpc(respawnPoint != null ? respawnPoint.position : Vector2.zero);
    }
}