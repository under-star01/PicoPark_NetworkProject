using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CeilCheck : MonoBehaviour
{
    public int ceilingCnt = 1; // 현재 어부바 인원수
    private HashSet<PlayerMove> ceilingSet = new HashSet<PlayerMove>();
    private PlayerMove playerMove;

    private void Awake()
    {
        playerMove = GetComponentInParent<PlayerMove>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMove player = other.GetComponentInParent<PlayerMove>();
        if (player == null) return;

        if (ceilingSet.Add(player))
        {
            UpdateStack();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMove player = other.GetComponentInParent<PlayerMove>();
        if (player == null) return;

        if (ceilingSet.Remove(player))
        {
            UpdateStack();
        }
    }

    private void UpdateStack()
    {
        int total = 1; // 자신 포함

        foreach (PlayerMove player in ceilingSet)
        {
            if (player.ceilCheck != null)
            {
                total += player.ceilCheck.ceilingCnt;
            }
        }

        ceilingCnt = total;

        // 아래로 전파
        if (playerMove.groundCheck != null &&
            playerMove.groundCheck.UnderPlayerRb != null &&
            playerMove.groundCheck.UnderPlayerRb.TryGetComponent<PlayerMove>(out var underPlayer))
        {
            underPlayer.ceilCheck.UpdateStack();
        }
    }
}
