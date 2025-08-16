using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class HpDamageEffectUI : NetworkBehaviour
{
    [SerializeField] HpComp target;
    [SerializeField] DamageText damageTextPrefab;
    private void OnEnable()
    {
        target.onDamage += OnDamage;
    }
    private void OnDisable()
    {
        target.onDamage -= OnDamage;
    }
    private void OnDamage(float damage) => OnDamageRpc(damage);
    [Rpc(SendTo.Everyone)]
    public void OnDamageRpc(float damage)
    {
        float angle = UnityEngine.Random.Range(0, 360.0f);
        damageTextPrefab.Get(transform.position, Quaternion.identity).Set(damage, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
    }
}