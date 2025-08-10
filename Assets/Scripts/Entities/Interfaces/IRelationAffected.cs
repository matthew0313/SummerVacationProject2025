using Unity.Netcode;
using UnityEngine;

public interface IRelationAffected
{
    public Entity owner { get; }
}