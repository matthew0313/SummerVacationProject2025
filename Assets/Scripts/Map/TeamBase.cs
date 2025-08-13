using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeamBase : NetworkBehaviour
{
    [SerializeField] NetworkAnimator anim;
    [SerializeField] UnitTime breakTime;

    [SerializeField] Team m_team;
    [SerializeField] float m_maxHp = 100.0f;
    public Team team => m_team;
    public float maxHp => m_maxHp;
    public readonly NetworkVariable<float> hp = new(100.0f);
    private void Awake()
    {
        GameManager.Instance.onRoundStart += () => { if (IsOwner) hp.Value = maxHp; };
    }
    readonly int damageID = Animator.StringToHash("Damage");
    readonly int breakID = Animator.StringToHash("Break");

    [Rpc(SendTo.Authority)]
    public void GetDamageRpc(float damage)
    {
        if (hp.Value <= 0) return;
        anim.SetTrigger(damageID);
        hp.Value = Mathf.Max(0, hp.Value - damage);
        if(hp.Value <= 0)
        {
            anim.SetTrigger(breakID);
            StartCoroutine(Breaking());
        }
    }
    IEnumerator Breaking()
    {
        yield return new WaitForSeconds(breakTime);
        if (team == Team.Blue) GameManager.Instance.EndRound(Team.Red);
        if (team == Team.Red) GameManager.Instance.EndRound(Team.Blue);
    }
}
