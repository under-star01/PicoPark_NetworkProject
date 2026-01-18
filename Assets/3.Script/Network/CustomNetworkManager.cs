using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        GameFlowManager.Instance.RegisterPlayer(conn);
        GameFlowManager.Instance.ApplyPendingMeta(conn);

        PlayerMove player = conn.identity.GetComponent<PlayerMove>();
        GameFlowManager.Instance.ApplyMetaToPlayer(player);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        GameFlowManager.Instance.OnServerSceneChanged(sceneName);
    }
}