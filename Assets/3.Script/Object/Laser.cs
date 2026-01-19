using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerMove player = collision.GetComponent<PlayerMove>();
        if (player == null) return;

        player.Die(); // 서버에서만 사망 처리
    }
}