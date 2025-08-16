using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeamBase : NetworkBehaviour
{
    [SerializeField] Team team;
    [SerializeField] TeamShop m_shop;
    [SerializeField] TeamGenerator m_generator;
    public TeamShop shop => m_shop;
    public TeamGenerator generator => m_generator;
    private void Start()
    {
        shop.onBreak += Check;
        generator.onBreak += Check;
    }
    void Check()
    {
        if (!HasAuthority) return;
        if(shop.broken.Value && generator.broken.Value)
        {
            if (team == Team.Red) GameManager.Instance.WinRound(Team.Blue);
            if (team == Team.Blue) GameManager.Instance.WinRound(Team.Red);
        }
    }
}
