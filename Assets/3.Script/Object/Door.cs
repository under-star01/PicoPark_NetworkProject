using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("문 스프라이트")]
    [SerializeField] private Sprite closedSprite; // 닫힌 문
    [SerializeField] private Sprite openedSprite; // 열린 문

    private SpriteRenderer spriteRenderer;
    private bool isOpened = false;

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 열렸으면 무시
        if (isOpened) return;

        // 열쇠와 충돌
        if (collision.gameObject.CompareTag("Key"))
        {
            OpenDoor();
            Destroy(collision.gameObject);//열쇠제거
        }
    }

    private void OpenDoor()
    {
        isOpened = true;
        spriteRenderer.sprite = openedSprite;
    }

    // 문 리셋
    public void ResetDoor()
    {
        isOpened = false;
        spriteRenderer.sprite = closedSprite;
    }
}