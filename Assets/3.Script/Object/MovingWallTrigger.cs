using Mirror;
using UnityEngine;

public class MovingWallTrigger : NetworkBehaviour
{
    private MovingWall wall;

    private void Awake()
    {
        wall = GetComponentInParent<MovingWall>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out PlayerMove player))
        {
            wall.AddTouchingPlayer(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out PlayerMove player))
        {
            wall.RemoveTouchingPlayer(player);
        }
    }
}