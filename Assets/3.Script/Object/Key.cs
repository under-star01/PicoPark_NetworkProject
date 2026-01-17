using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Key : MonoBehaviour
{
    private PlayerMove currentOwner; // 현재 열쇠를 가진 플레이어
    private bool canPickup = true; // 픽업 가능 여부
    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void FixedUpdate()
    {
        // 주인이 있으면 따라가기
        if (currentOwner != null)
        {
            FollowOwner();
        }
    }

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

    private void FollowOwner()
    {
        if (currentOwner == null) return;

        // 플레이어 뒤쪽 위치 계산
        Vector2 pos = GetFollowPosition();

        // 부드럽게 이동
        Vector2 newPos = Vector2.Lerp(rb.position, pos, 5f * Time.fixedDeltaTime); // 속도 =5f
        rb.MovePosition(newPos);
    }

    private Vector2 GetFollowPosition()
    {
        // 플레이어가 보는 방향의 반대쪽 뒤로 위치
        SpriteRenderer playerSprite = currentOwner.GetComponent<SpriteRenderer>();
        float direction = playerSprite.flipX ? 1f : -1f; // flipX가 true면 왼쪽을 보고 있음

        Vector2 ownerPos = currentOwner.transform.position;
        Vector2 offset = new Vector2(direction * 0.3f, 0.5f);//플레이 뒤에서 살짝 위

        return ownerPos + offset;
    }

    private void SetOwner(PlayerMove newOwner)
    {
        currentOwner = newOwner;

        // 픽업 딜레이
        StartCoroutine(PickupDelay_co());
    }

    private IEnumerator PickupDelay_co()//픽업딜레이
    {
        canPickup = false;
        yield return new WaitForSeconds(0.2f);
        canPickup = true;
    }

    public void ResetKey(Vector2 position)
    {
        currentOwner = null;
        transform.position = position;
        canPickup = true;
    }
}