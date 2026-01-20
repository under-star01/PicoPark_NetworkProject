using UnityEngine;
using Mirror;

public class SavePoint : NetworkBehaviour
{
    public Transform returnPos;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;

        if (!other.TryGetComponent<PlayerMove>(out var player))
            return;

        player.SetReturnPos(returnPos);
    }
}
