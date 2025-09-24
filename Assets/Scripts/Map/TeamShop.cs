using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeamShop : MortarTarget
{
    [SerializeField] NetworkAnimator anim;
    [SerializeField] int regenTime = 30;
    [SerializeField] float interactRange = 3.0f;
    [SerializeField] GameObject interactIndicator;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    public readonly NetworkVariable<int> regenCounter = new(30);
    ShopTab shopTab => GameUIManager.Instance.shopTab;
    readonly int brokenID = Animator.StringToHash("Broken");

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
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
            interactIndicator.SetActive(false);
        }
        else
        {
            float dist = Vector2.Distance(transform.position, Player.localPlayer.transform.position);
            if (dist <= interactRange)
            {
                interactIndicator.SetActive(true);
                if (Input.GetKeyDown(interactKey))
                {
                    GameUIManager.Instance.OpenTab(GameUIManager.Instance.shopTab);
                }
            }
            else interactIndicator.SetActive(false);
        }
    }
}