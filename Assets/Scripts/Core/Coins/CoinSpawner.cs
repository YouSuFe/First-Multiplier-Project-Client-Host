using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private int coinValue = 10;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private ContactFilter2D contactFilter;
    private float coinRadius;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Initialize coin radius and contact filter
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;
        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true // If you want to include triggers
        };
        contactFilter.SetLayerMask(layerMask);

        // Spawn initial coins
        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
        coinInstance.SetValue(coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn();

        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        while (true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);

            // Check for overlapping objects at the spawn point
            int numColliders = Physics2D.OverlapCircle(spawnPoint, coinRadius, contactFilter, coinBuffer);

            // If no colliders, return the spawn point
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
