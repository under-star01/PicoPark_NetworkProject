using UnityEngine;
using Mirror;

public class StageCheckpointManager : NetworkBehaviour
{
    public static StageCheckpointManager Instance;

    [SerializeField]
    private Transform currentCheckpoint;

    public override void OnStartServer()
    {
        Instance = this;
    }

    [Server]
    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    [Server]
    public Vector3 GetRespawnPosition()
    {
        return currentCheckpoint != null
            ? currentCheckpoint.position
            : Vector3.zero;
    }
}