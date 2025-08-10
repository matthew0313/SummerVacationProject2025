using Unity.Netcode;
using UnityEngine;

public interface IDamagable : IRelationAffected
{
    public void GetDamage(float damage);
}