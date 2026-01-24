using Mirror;
using UnityEngine;

public class NetworkAudio : NetworkBehaviour
{
    public static NetworkAudio Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // 클라이언트가 호출
    public void PlaySharedSFX(string name)
    {
        CmdPlaySharedSFX(name);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlaySharedSFX(string name)
    {
        RpcPlaySharedSFX(name);
    }

    [ClientRpc]
    private void RpcPlaySharedSFX(string name)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayLocalSFX(name);
    }

    // 클리어 같은 서버 1회용
    [ClientRpc]
    public void RpcPlayClearOnce()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayLocalSFX("Clear");
    }
}