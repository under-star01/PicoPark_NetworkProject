using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
    public bool IsGround { get; private set; }
    public Rigidbody2D UnderPlayerRb { get; private set; }

    private int groundCount = 0;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundCount++;
        IsGround = true;

        // 밟고 있는 게 플레이어라면
        if (other.CompareTag("Player"))
        {
            // 자기 자신 제외
            if (other.gameObject != transform.root.gameObject)
            {
                UnderPlayerRb = other.attachedRigidbody;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundCount--;
        if (groundCount <= 0)
        {
            groundCount = 0;
            IsGround = false;
            UnderPlayerRb = null;
        }
    }
}
