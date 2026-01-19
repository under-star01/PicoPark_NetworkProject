using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerStageController : NetworkBehaviour
{
    [Command]
    public void CmdRequestRetry()
    {
        NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }

    [Command]
    public void CmdReturnToLobby()
    {
        OnlineMenu_UIManager.shouldShowStageSelect = true;
        NetworkManager.singleton.ServerChangeScene("2.Lobby");
    }
}
