using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private const string MenuSceneName = "Menu";

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Here when the client is disconnected, we need to shutdown the network and change the scene
    private void OnClientDisconnect(ulong clientId)
    {
        // Means Host is disconnected
        if (clientId != 0 && clientId != networkManager.LocalClientId) return;

        Disconnect();
    }

    public void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
            Debug.LogWarning("Shutting Down");
        }
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

}

/*
private const string MenuSceneName = "Menu";

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Called when the client disconnects
    private async void OnClientDisconnect(ulong clientId)
    {
        // Means Host is disconnected
        if (clientId != 0 && clientId != networkManager.LocalClientId) return;

        Debug.Log($"Client {clientId} disconnected. Starting cleanup process...");

        // Ensure the scene is back to the Menu
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            Debug.Log("Switching to Menu scene...");
            SceneManager.LoadScene(MenuSceneName);
        }

        // Ensure the client is properly disconnected from Relay and Lobby
        if (networkManager.IsConnectedClient)
        {
            try
            {
                // Step 1: Leave the lobby if the player is signed in
                if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Leaving the lobby...");
                    await LeaveLobbyAsync();
                }

                // Step 2: Shutdown the Relay transport layer
                UnityTransport transport = networkManager.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.Shutdown();
                    Debug.Log("Relay transport shutdown successfully.");
                }

                // Step 3: Shutdown the NetworkManager
                networkManager.Shutdown();
                Debug.Log("NetworkManager shutdown successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during cleanup: {ex}");
            }
        }
    }

    private async Task LeaveLobbyAsync()
    {
        string lobbyId = HostSingleton.Instance?.GameManager?.lobbyId ?? string.Empty;

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                // Determine if this client is the host based on LocalClientId
                bool isHost = NetworkManager.Singleton.LocalClientId == 0;

                if (isHost)
                {
                    // If this client is the host, delete the lobby
                    await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
                    Debug.Log("Lobby deleted successfully by the host.");
                }
                else
                {
                    // Otherwise, remove the client from the lobby
                    await Lobbies.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);
                    Debug.Log("Player removed from the lobby successfully.");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to leave lobby: {e}");
            }
        }
        else
        {
            Debug.LogWarning("No lobby ID found. Skipping lobby leave.");
        }
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
*/