using Unity.Netcode;
using UnityEngine;

public class TeamBase : NetworkBehaviour
{
    [SerializeField] Team m_team;
    [SerializeField] float m_maxHp = 100.0f;
    public Team team => m_team;
    public float maxHp => m_maxHp;
    public readonly NetworkVariable<float> hp = new(100.0f);
    private void Awake()
    {
        GameManager.Instance.onRoundStart += () => { if (IsOwner) hp.Value = maxHp; };
    }
}
