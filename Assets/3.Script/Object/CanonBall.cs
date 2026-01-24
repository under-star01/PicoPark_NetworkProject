using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CanonBall : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private Vector2 pushVelocity = new Vector2(5f, 3f); // 플레이어 넉백 속도 (x, y)

    private Rigidbody2D rb;
    private Animator ani;

    private bool isActive = false; // 현재 발사 중인지 확인
    [SyncVar] private bool shootLeft = true; // Canon에서 설정됨

    private Canon ownerCanon;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out ani);
    }

    [Server]
    public void Init(bool left, Canon canon)
    {
        shootLeft = left;
        ownerCanon = canon;
        isActive = true;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isActive) return;

        Vector2 dir = shootLeft ? Vector2.left : Vector2.right;
        rb.linearVelocity = dir * shootSpeed;
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        // 플레이어 충돌
        if (collision.CompareTag("Player"))
        {
            NetworkAudio.Instance.PlaySharedSFX("CanonHit");
            PlayerMove player = collision.GetComponent<PlayerMove>();
            if (player != null)
            {
                float pushDir = shootLeft ? -1f : 1f;
                Vector2 knockback = new Vector2(pushDir * pushVelocity.x, pushVelocity.y);

                player.Knockback(knockback);
            }

            HitServer();
            return;
        }

        // 벽 충돌
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            NetworkAudio.Instance.PlaySharedSFX("CanonHit");

            HitServer();
        }
    }

    [Server]
    private void HitServer()
    {
        if (!isActive) return;

        isActive = false;
        rb.linearVelocity = Vector2.zero;

        RpcPlayHitAnim();
    }


    [ClientRpc]
    private void RpcPlayHitAnim()
    {
        if (ani != null)
            ani.SetTrigger("Hit");
    }

    [Server]
    private void DestroySelf()
    {
        if (ownerCanon != null)
            ownerCanon.OnCanonBallDestroyed();

        NetworkServer.Destroy(gameObject);
    }

    public void AnimHitEnd()
    {
        if (!isServer) return;
        DestroySelf();
    }
}