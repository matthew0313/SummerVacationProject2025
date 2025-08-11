using Unity.Netcode;
using UnityEngine;

public interface IDamagable : IRelationAffected
{
    public void GetDamage(DamageInfo damage);
}
[System.Serializable]
public struct DamageInfo : INetworkSerializable
{
    public float amount;
    public ulong damageGiverClientID;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref amount);
        serializer.SerializeValue(ref damageGiverClientID);
    }
}