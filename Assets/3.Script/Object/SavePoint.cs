using UnityEngine;
using Mirror;

public class SavePoint : NetworkBehaviour
{
    public Transform returnPos;

    [SyncVar]
    private bool isActivated = false;

    [Server]
    public bool TryActivate()
    {
        if (isActivated)
            return false;

        isActivated = true;
        return true;
    }
}
