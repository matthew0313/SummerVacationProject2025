using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeamGenerator : MortarTarget
{
    public const int maxTier = 3;

    [SerializeField] NetworkAnimator anim;
    [SerializeField] int regenTime = 30;
    [SerializeField] GeneratorStats[] statsPerTier = new GeneratorStats[maxTier];
    [SerializeField] GasPickup gasPickupPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float minForce, maxForce;
    public readonly NetworkList<NetworkBehaviourReference> spawnedGas = new();
    public readonly NetworkVariable<int> regenCounter = new(30);
    public readonly NetworkVariable<int> upgradeTier = new(0);

    readonly int brokenID = Animator.StringToHash("Broken");
    public int upgradeCost => statsPerTier[upgradeTier.Value].upgradeCost;

    [Rpc(SendTo.Authority)]
    public void UpgradeRpc(int targetTier, RpcParams rpcParams = new())
    {
        if (targetTier > maxTier || targetTier != upgradeTier.Value + 1) return;
        GameManager.Instance.GetPlayer(rpcParams.Receive.SenderClientId).inventory.AddBoltRpc(-upgradeCost);
        upgradeTier.Value += 1;
    }
    private void Start()
    {
        GameManager.Instance.onRoundStart += () =>
        {
            if (!HasAuthority) return;
            upgradeTier.Value = 0;
        };
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
        if (HasAuthority)
        {
            anim.SetBool(brokenID, true);
            regenCounter.Value = regenTime;
        }
    }
    float regenTickCounter = 0.0f;
    float counter = 0.0f;
    float spawnRate => statsPerTier[upgradeTier.Value].spawnRate;
    int maxSpawned => statsPerTier[upgradeTier.Value].maxSpawned;
    private void Update()
    {
        if (!HasAuthority || GameManager.Instance.gameState.Value != GameState.RoundInProgress) return;
        for(int i = 0; i < spawnedGas.Count; i++)
        {
            if (!spawnedGas[i].TryGet(out GasPickup gas))
            {
                spawnedGas.RemoveAt(i); i--;
            }
        }
        if (broken.Value)
        {
            regenTickCounter += Time.deltaTime;
            if (regenTickCounter >= 1.0f)
            {
                regenTickCounter -= 1.0f;
                if (--regenCounter.Value <= 0)
                {
                    broken.Value = false;
                    anim.SetBool(brokenID, false);
                }
            }
        }
        else
        {
            if (counter < spawnRate) counter += Time.deltaTime;
            if(counter >= spawnRate)
            {
                counter -= spawnRate;
                SpawnGas();
            }
        }
    }
    void SpawnGas()
    {
        if (spawnedGas.Count >= maxSpawned) return;
        GasPickup tmp = Instantiate(gasPickupPrefab, spawnPoint.position, Quaternion.identity);
        float angle = UnityEngine.Random.Range(0, 360.0f);
        float force = UnityEngine.Random.Range(minForce, maxForce);
        tmp.rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * force;
        tmp.NetworkObject.Spawn(true);
        spawnedGas.Add(tmp);
    }
}
[System.Serializable]
public struct GeneratorStats
{
    public float spawnRate;
    public int maxSpawned;
    public int upgradeCost;
}