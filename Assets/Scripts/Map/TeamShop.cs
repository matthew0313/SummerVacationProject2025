using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeamShop : MortarTarget
{
    [SerializeField] NetworkAnimator anim;
    [SerializeField] int regenTime = 30;
    public readonly NetworkVariable<int> regenCounter = new(30);
    ShopTab shopTab => GameUIManager.Instance.shopTab;
    readonly int brokenID = Animator.StringToHash("Broken");

    private void Start()
    {
        GameManager.Instance.onRoundEnd += () =>
        {
            if (!HasAuthority) return;
            broken.Value = false;
            anim.SetBool(brokenID, false);
        };
    }
    protected override void OnBreak()
    {
        base.OnBreak();
        if(HasAuthority) 
        { 
            anim.SetBool(brokenID, true);
            regenCounter.Value = regenTime;
        }
        if(GameUIManager.Instance.currentTab == shopTab) GameUIManager.Instance.CloseTab();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Player.localPlayer == null || broken.Value) return;
        GameObject hit = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
        if(hit == Player.localPlayer.gameObject && !GameUIManager.Instance.isTabOpen)
        {
            GameUIManager.Instance.OpenTab(shopTab);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Player.localPlayer == null) return;
        GameObject hit = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
        if (hit == Player.localPlayer.gameObject && GameUIManager.Instance.currentTab == shopTab)
        {
            GameUIManager.Instance.CloseTab();
        }
    }
    float counter = 0.0f;
    private void Update()
    {
        if (!HasAuthority || GameManager.Instance.gameState.Value != GameState.RoundInProgress) return;
        if (broken.Value)
        {
            counter += Time.deltaTime;
            if (counter >= 1.0f)
            {
                counter -= 1.0f;
                if (--regenCounter.Value <= 0)
                {
                    broken.Value = false;
                    anim.SetBool(brokenID, false);
                }
            }
        }
    }
}