using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{

    private const string GameSceneName = "Game";
    private const int MaxConnections = 20;

    private Allocation allocation;

    private string joinCode;

    private string lobbyId;

    public NetworkServer NetworkServer { get; private set; }

    public async Task StartHostAsync()
    {
        try
        {
            // To create the max connection for Relay, to create allocation
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception exception)
        {
            Debug.Log(exception);
            return;
        }

        try
        {
            // To connect other players, we need join code for particular allocation
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (Exception exception)
        {
            Debug.Log(exception);
            return;
        }


        // We then set the data on the transport so it's ready to go when we host the server.
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        // Lobby Related
        // But just before host the server that we'll create a lobby,
        // assign our join code to the lobby, and over here we create it
        // And as soon as we create it, we start this 15 second interval between pinging the server.
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: joinCode
                        )
                }
            };

            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);

            lobbyId = lobby.Id;

            Debug.Log($"Lobby created successfully: {lobby.Name} (ID: {lobby.Id})");

            // Unity Doc said, if we want to keep lobby stay, we need to use SendHearthbeatPingAsync to keep it alive
            // We are doing it inside HostSingleton beacuse it will be active in the scene and we cannot call StartCoroutine here
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        // Lobby ends

        // For connection approval, we created NetworkServer to handle that
        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        // To make connection with correct user name we entered on Bootsrap scene,
        // We convert data to byte and pass it into NetworkManager.
        // We did the exactly reverse in getting data in NetworkServer class
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;


        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }


    public void Dispose()
    {
        ShutDown();
    }

    public async void ShutDown()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            // If it tries to delete the lobby twice, just in case we put empty
            lobbyId = string.Empty;
        }

        NetworkServer.OnClientLeft -= HandleClientLeft;

        NetworkServer?.Dispose();
    }


}
