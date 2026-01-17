using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Key : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnOwnerChanged))]
    private NetworkIdentity ownerNetId;

    private PlayerMove currentOwner; // 현재 열쇠를 가진 플레이어
    private bool canPickup = true; // 픽업 가능 여부
    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        // 주인이 있으면 따라가기
        if (currentOwner != null)
        {
            FollowOwner();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 픽업 가능 상태가 아니면 무시
        if (!canPickup) return;

        // 플레이어와 충돌
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMove newOwner = collision.gameObject.GetComponent<PlayerMove>();

            // 새로운 플레이어이거나 주인이 없을 때만 변경
            if (newOwner != null && newOwner != currentOwner)
            {
                SetOwner(newOwner);
            }
        }
    }

    [Server]
    private void FollowOwner()
    {
        Vector2 targetPos = GetFollowPosition();
        rb.MovePosition(Vector2.Lerp(rb.position, targetPos, 5f * Time.fixedDeltaTime));
    }

    private Vector2 GetFollowPosition()
    {
        // 플레이어가 보는 방향의 반대쪽 뒤로 위치
        float direction = currentOwner.transform.localScale.x > 0 ? -1f : 1f;

        Vector2 ownerPos = currentOwner.transform.position;
        Vector2 offset = new Vector2(direction * 0.3f, 0.5f);//플레이 뒤에서 살짝 위

        return ownerPos + offset;
    }

    [Server]
    private void SetOwner(PlayerMove newOwner)
    {
        currentOwner = newOwner;
        ownerNetId = newOwner.netIdentity;

        // 픽업 딜레이
        StartCoroutine(PickupDelay_co());
    }

    [Server]
    private IEnumerator PickupDelay_co()
    {
        canPickup = false;
        yield return new WaitForSeconds(0.2f);
        canPickup = true;
    }

    [Server]
    public void ResetKey(Vector2 position)
    {
        currentOwner = null;
        ownerNetId = null;

        rb.linearVelocity = Vector2.zero;
        rb.position = position;

        canPickup = true;
    }
    private void OnOwnerChanged(NetworkIdentity oldOwner, NetworkIdentity newOwner)
    {
        currentOwner = newOwner ? newOwner.GetComponent<PlayerMove>() : null;
    }
}