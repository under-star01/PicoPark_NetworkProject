using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager_MH : NetworkManager
{
    public static CustomNetworkManager_MH Instance;

    public Dictionary<int, PlayerMetaData> playerMetas = new();
    private int nextPlayerIndex = 0;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (playerMetas.ContainsKey(conn.connectionId))
            return;

        playerMetas.Add(conn.connectionId,
            new PlayerMetaData(conn.connectionId, nextPlayerIndex++)
        );

        Debug.Log($"[Server] Connected connId={conn.connectionId}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (!playerMetas.TryGetValue(conn.connectionId, out var meta))
            return;

        meta.netId = conn.identity.netId;

        PlayerMove player = conn.identity.GetComponent<PlayerMove>();
        player.ApplyMetaData(meta);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            if (!playerMetas.TryGetValue(conn.connectionId, out var meta))
                continue;

            conn.identity
                .GetComponent<PlayerMove>()
                .ApplyMetaData(meta);
        }
    }

    // 서버 전용 API
    public void SetPlayerAppearance(int connId, int color, int hat)
    {
        if (playerMetas.TryGetValue(connId, out var meta))
        {
            meta.colorIndex = color;
            meta.hatIndex = hat;
        }
    }
}
