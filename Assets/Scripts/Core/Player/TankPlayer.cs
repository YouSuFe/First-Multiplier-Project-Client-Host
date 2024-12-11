using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using Unity.Collections;
using System;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn called.");

        if (IsServer)
        {
            Debug.Log("OnNetworkSpawn called from server.");

            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            cinemachineCamera.Priority = ownerPriority;
            minimapIconRenderer.color = ownerColor;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            Debug.Log("OnNetworkDeSpawn called from server.");

            OnPlayerDespawned?.Invoke(this);
        }
    }
}
