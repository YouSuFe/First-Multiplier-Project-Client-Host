using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }



    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // We are getting byte[] array, So we need to convert it and convert back to byte as well
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        // When we get the name here, after this point, it does not being stored.
        // It will be thrown away by system, so we need to keep it somewhere
        UserData userData;
        try
        {
            // Attempt to deserialize the payload
            userData = JsonUtility.FromJson<UserData>(payload);
        }
        catch
        {
            Debug.LogError("Failed to deserialize user data payload.");
            response.Approved = false; // Reject the connection if payload is invalid
            return;
        }

        // clientIdToAuth.Add(request.ClientNetworkId, userData.userAuthId) ,
        // The below means that if there is no this client id, create and asign,
        // So, we do not need to explicitly Add into dictionary if it is not exist.
        // With this, if there is none, create; if there is, change it.
        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        // Same as above
        authIdToUserData[userData.userAuthId] = userData;

        response.Approved = true;

        // Where will the player be spawned on the scene (except host),
        // Host will be spawned on the (0,0,0) when the server first start
        response.Position = SpawnPoint.GetRandomSpawnPosition();
        response.Rotation = Quaternion.identity;

        // When we adjust the Approval Check in Network Manager,
        // We need to create the player object with this code,
        // beacuse Network Manager make it false when we change approval check automatically
        response.CreatePlayerObject = true;

        Debug.LogWarning($"Approval Request: ClientId {request.ClientNetworkId}, Payload {payload}");

    }

    // This method is called once we've basically started up the server, once it's ready to go
    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Here we can actually choose whether we want to remove data once player disconnect,
    // even if he comes back, there will be no data about or progress he made.
    // Or, we can keep storing it 
    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);

            authIdToUserData.Remove(authId);

            OnClientLeft?.Invoke(authId);
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }

            return null;
        }

        return null;
    }


    public void Dispose()
    {
        if (networkManager != null)
        {
            Debug.LogWarning("From NetworkServer, networkManager is null");
            return;
        }

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
