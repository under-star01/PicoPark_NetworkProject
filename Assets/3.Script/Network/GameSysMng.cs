using Mirror;
using UnityEngine;

public class GameSystemManager : NetworkBehaviour
{
    public static GameSystemManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ClientRpc] // 모든 클라이언트에게 시스템 메시지 전송
    public void RpcNotifySystemMessage(string message)
    {
        //구현 시 UIManager.Instance.ShowAlert(message) 등으로 대체
    }
}