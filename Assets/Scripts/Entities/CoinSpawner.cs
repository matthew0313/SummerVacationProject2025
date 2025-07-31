using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject coinPrefab;
    public float spawnRadius = 5f;
    public int maxCoins = 10;
    public float spawnInterval = 1f; // 초마다 생성

    private List<GameObject> spawnedCoins = new();
    private float spawnTimer;

    private void Update()
    {
        if (!IsOwner) return;

        spawnTimer += Time.deltaTime;

        // 초마다 한 번 시도
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            CleanupDestroyedCoins();

            if (spawnedCoins.Count < maxCoins)
            {
                TrySpawnCoin();
            }
        }
    }

    private void TrySpawnCoin()
    {
        Vector3 randomPos = transform.position + (Vector3)Random.insideUnitCircle * spawnRadius;
        GameObject coin = Instantiate(coinPrefab, randomPos, Quaternion.identity);

        // Netcode를 통해 전체 클라이언트에 스폰
        coin.GetComponent<NetworkObject>().Spawn(true); // true = 소유권을 Spawner Owner에게 부여

        spawnedCoins.Add(coin);
    }

    private void CleanupDestroyedCoins()
    {
        spawnedCoins.RemoveAll(c => c == null);
    }
}