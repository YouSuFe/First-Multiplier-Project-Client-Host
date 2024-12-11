using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin bountyCoinPrefab;

    [Header("Settings")]
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private ContactFilter2D contactFilter;
    private float coinRadius;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true // If you want to include triggers
        };
        contactFilter.SetLayerMask(layerMask);

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        health.OnDie -= HandleDie;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out Coin coin)) return;

        int coinValue = coin.Collect();

        if (!IsServer) return;

        TotalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }

    private void HandleDie(Health obj)
    {
        int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) return;

        for(int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(bountyCoinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {

            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;

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
