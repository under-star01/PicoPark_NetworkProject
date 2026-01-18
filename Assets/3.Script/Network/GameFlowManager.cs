using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[Serializable]
public class PlayerMetaData
{
    public uint netId;          // NetworkIdentity.netId
    public int connectionId;    // Mirror connectionId
    public int colorIndex;      // 색상
    public int hatIndex;        // 모자
}

public enum GameState
{
    Title,
    Lobby,
    Playing,
    StageClear,
    GameOver
}

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance;

    [SyncVar]
    public GameState CurrentState = GameState.Title;

    private Dictionary<uint, PlayerMetaData> playerData = new();

    private Dictionary<int, (int color, int hat)> pendingMeta = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void SetPendingMeta(NetworkConnectionToClient conn, int colorIndex, int hatIndex)
    {
        pendingMeta[conn.connectionId] = (colorIndex, hatIndex);
    }

    [Server]
    public void ApplyPendingMeta(NetworkConnectionToClient conn)
    {
        if (!pendingMeta.TryGetValue(conn.connectionId, out var meta))
            return;

        SetPlayerMetaData(conn, meta.color, meta.hat);
        pendingMeta.Remove(conn.connectionId);
    }

    [Server]
    public void RegisterPlayer(NetworkConnectionToClient conn)
    {
        uint netId = conn.identity.netId;
        if (playerData.ContainsKey(netId)) return;

        playerData.Add(netId, new PlayerMetaData
        {
            netId = netId,
            connectionId = conn.connectionId,
            colorIndex = 0,
            hatIndex = 0
        });
    }

    [Server]
    public void SetPlayerMetaData(NetworkConnectionToClient conn, int colorIndex, int hatIndex)
    {
        uint netId = conn.identity.netId;

        if (!playerData.ContainsKey(netId))
            RegisterPlayer(conn);

        playerData[netId].colorIndex = colorIndex;
        playerData[netId].hatIndex = hatIndex;
    }

    [Server]
    public void ApplyMetaToPlayer(PlayerMove player)
    {
        uint netId = player.netIdentity.netId;

        if (!playerData.TryGetValue(netId, out var data))
            return;

        player.ApplyMetaData(data.colorIndex, data.hatIndex);
    }

    [Server]
    public void StartGame(int stageIndex)
    {
        if (CurrentState != GameState.Lobby) return;

        CurrentState = GameState.Playing;
        NetworkManager.singleton.ServerChangeScene($"Scene_Stage{stageIndex}");
    }

    [Server]
    public void OnServerSceneChanged(string sceneName)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            PlayerMove player = conn.identity.GetComponent<PlayerMove>();
            ApplyMetaToPlayer(player);
        }
    }
}