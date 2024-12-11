using Unity.Netcode;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    // It is for pure Host/Client logic, There is not such a thing for dedicated server logic
    public void LeaveGame()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.ShutDown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }
}
