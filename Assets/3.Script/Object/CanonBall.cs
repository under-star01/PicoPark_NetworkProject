using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBall : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private Vector2 pushVelocity = new Vector2(5f, 3f); // 플레이어 넉백 속도 (x, y)

    private Rigidbody2D rb;
    private Animator ani;
    private bool isActive = false; // 현재 발사 중인지 확인
    private bool shootLeft = true; // Canon에서 설정됨

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out ani);
    }

    private void OnEnable()
    {
        isActive = true;
        //rb.bodyType = RigidbodyType2D.Dynamic; // 발사 시 Dynamic으로 변경
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            Shoot();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        // 플레이어와 충돌 시 밀어내기
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMove playerMove = collision.gameObject.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // 충돌 방향 계산
                float pushDirection = Mathf.Sign(collision.transform.position.x - transform.position.x);
                Vector2 knockback = new Vector2(pushDirection * pushVelocity.x, pushVelocity.y);

                // 넉백 함수 호출
                playerMove.Knockback(knockback);
            }
            Hit();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Hit();
        }
    }

    private void Hit()
    {
        isActive = false;
        rb.linearVelocity = Vector2.zero; // 속도 초기화
        //rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic으로 변경하여 물리 영향 제거
        ani.SetTrigger("Hit");
    }

    private void Shoot()
    {
        Vector2 direction;

        if (shootLeft)
        {
            direction = Vector2.left;
        }
        else
        {
            direction = Vector2.right;
        }

        rb.linearVelocity = direction * shootSpeed;
    }

    // 방향 설정
    public void SetDirection(bool left)
    {
        shootLeft = left;
    }

    // 애니메이션 이벤트에서 호출할 함수
    public void Reload()
    {
        gameObject.SetActive(false);
    }
}