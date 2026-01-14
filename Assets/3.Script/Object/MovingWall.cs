using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [SerializeField] private int targetMoveCnt = 2;
    [SerializeField] private int pushCnt = 0;
    public HashSet<PlayerMove> pushers = new HashSet<PlayerMove>();

    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = 0f;

        // 디버그용!
        pushCnt = pushers.Count;
    }
    public void AddPusher(PlayerMove p)
    {
        pushers.Add(p);

        if (pushers.Count >= targetMoveCnt)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void RemovePusher(PlayerMove p)
    {
        pushers.Remove(p);

        if (pushers.Count < targetMoveCnt)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
