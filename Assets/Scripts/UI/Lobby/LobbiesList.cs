using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) return;

        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            // How many result we want
            options.Count = 25;

            options.Filters = new List<QueryFilter>()
            {
                /*
                 * So we're saying here, go check the available slots for this lobby and make sure they are greater than zero.
                 * All this means is just make sure there's room in the lobby if you're going to show it on the list.
                 */
                new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    ),
                /*
                 * So we're saying here, go check lobby if it is locked and if it is locked which is equal.
                 * All this means is just make sure room is locked in the lobby and we cannot show it on the list.
                 */
                new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0"
                    ),
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            // Destroy the child (lobbies UI) before instantiate
            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }


            // Create Lobbies UI for lobbies
            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);

                lobbyItem.Initialize(this, lobby);
            }

        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) return;

        isJoining = true;

        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        isJoining = false;
    }
}
