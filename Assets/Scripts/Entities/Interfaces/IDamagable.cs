using Unity.Netcode;
using UnityEngine;

public interface IDamagable
{
    public Entity entity { get; }
    public void GetDamage(float damage);
}