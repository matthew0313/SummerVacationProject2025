using Unity.Netcode;
using UnityEngine;

public class BoltGenerator : NetworkBehaviour
{
    [SerializeField] float spawnRate = 10.0f;
    [SerializeField] int maxSpawned = 10;
    [SerializeField] BoltPickup boltPickupPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float minForce, maxForce;
    public readonly NetworkList<NetworkBehaviourReference> spawnedBolts = new();
    float counter = 0.0f;

    private void Update()
    {
        if (!HasAuthority || GameManager.Instance.gameState.Value != GameState.RoundInProgress) return;
        for (int i = 0; i < spawnedBolts.Count; i++)
        {
            if (!spawnedBolts[i].TryGet(out BoltPickup bolt))
            {
                spawnedBolts.RemoveAt(i); i--;
            }
        }
        if (counter < spawnRate) counter += Time.deltaTime;
        if (counter >= spawnRate)
        {
            counter -= spawnRate;
            SpawnBolt();
        }
    }
    void SpawnBolt()
    {
        if (spawnedBolts.Count >= maxSpawned) return;
        BoltPickup tmp = Instantiate(boltPickupPrefab, spawnPoint.position, Quaternion.identity);
        float angle = UnityEngine.Random.Range(0, 360.0f);
        float force = UnityEngine.Random.Range(minForce, maxForce);
        tmp.rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * force;
        tmp.NetworkObject.Spawn(true);
        spawnedBolts.Add(tmp);
    }
}