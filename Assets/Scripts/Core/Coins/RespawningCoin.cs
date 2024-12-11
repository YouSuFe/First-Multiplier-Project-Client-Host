using System;
using UnityEngine;

public class RespawningCoin : Coin
{
    // Here we have a collected event but not inside the bounty,
    // Beacuse, we are respawning this network object with Network Transform component
    // that's why we need to keep track their positions over scene
    // However, for bounty coins, we just need them once and destroy them.
    // That is why we do not have Network Transfrom on them, so no Event as well.
    public event Action<RespawningCoin> OnCollected;

    private Vector3 previousPosition;

    private void Update()
    {
        if (previousPosition != transform.position)
        {
            Show(true);
        }

        previousPosition = transform.position; 
    }

    public override int Collect()
    {
        if(!IsServer)
        {
            Show(false);
            return 0;
        }

        if (alreadyCollected) return 0;

        alreadyCollected = true;

        OnCollected?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
}
