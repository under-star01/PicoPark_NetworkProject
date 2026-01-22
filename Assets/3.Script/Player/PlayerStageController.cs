using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerStageController : NetworkBehaviour
{
    [Command]
    public void CmdRequestRetry()
    {
        if (!connectionToClient.identity.isServer) return; // Host만 허용
        NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }

    [Command]
    public void CmdReturnToLobby()
    {
        if (!connectionToClient.identity.isServer) return; // Host만 허용
        OnlineMenu_UIManager.shouldShowStageSelect = true;
        NetworkManager.singleton.ServerChangeScene("2.Lobby");
    }
}
