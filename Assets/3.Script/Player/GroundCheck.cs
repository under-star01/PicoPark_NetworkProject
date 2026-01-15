using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
    public bool IsGround { get; private set; }
    public Rigidbody2D UnderPlayerRb;

    private HashSet<Collider2D> groundSet = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundSet.Add(other);
        IsGround = groundSet.Count > 0;

        RecalculateUnderPlayer();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.isTrigger) return;

        // 이미 겹쳐 있으면 땅으로 인정
        if (!groundSet.Contains(other))
        {
            groundSet.Add(other);
            IsGround = true;
            RecalculateUnderPlayer();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundSet.Remove(other);

        IsGround = groundSet.Count > 0;

        RecalculateUnderPlayer();
    }

    private void RecalculateUnderPlayer()
    {
        UnderPlayerRb = null;

        foreach (var col in groundSet)
        {
            if (col == null) continue;
            if (!col.CompareTag("Player")) continue;
            if (col.transform.root.gameObject == transform.root.gameObject) continue;

            float checkBottom = GetComponent<Collider2D>().bounds.min.y; // 콜라이더 가장 아래 좌표값
            float colTop = col.bounds.max.y; // 부딪힌 ground 오브젝트의 가장 위에 좌표값

            if (colTop <= checkBottom + 0.05f) // 아래에 있을 때만 바닥으로 인정
            {
                UnderPlayerRb = col.attachedRigidbody;
                return;
            }
        }
    }
}
