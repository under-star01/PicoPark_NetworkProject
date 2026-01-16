using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonOb : MonoBehaviour
{
    [Header("버튼 타입")]
    [SerializeField] private ButtonType buttonType = ButtonType.Toggle;

    [Header("버튼 스프라이트")]
    [SerializeField] private SpriteRenderer buttonSR;
    [SerializeField] private Sprite pressed; // 눌린 스프라이트
    [SerializeField] private Sprite notpressed; // 안 눌린 스프라이트

    [Header("이벤트")]
    [SerializeField] private UnityEvent onPress; // 활성화 시
    [SerializeField] private UnityEvent offPress; // 비활성화 시

    private bool isPressed = false;
    private int playerCount = 0; // 버튼 위에 있는 플레이어 수

    public enum ButtonType
    {
        Toggle,  // 한번 누르면 작동
        Hold     // 누르는 동안만 작동
    }

    private void Awake()
    {
        buttonSR.sprite = notpressed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerCount++;

            if (playerCount == 1) // 첫 플레이어가 들어왔을 때
            {
                Press(); //눌러잇
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerCount--;

            if (playerCount <= 0) // 모든 플레이어가 나갔을 때
            {
                playerCount = 0;

                if (buttonType == ButtonType.Hold) //홀드만
                {
                    Release(); //놔
                }
            }
        }
    }

    // 버튼 누름
    private void Press()
    {
        if (buttonType == ButtonType.Toggle && isPressed) return; // Toggle은 이미 눌렸으면 무시

        isPressed = true;
        UpdateSprite();
        onPress?.Invoke();
    }

    // 버튼 해제 (홀드만)
    private void Release()
    {
        if (!isPressed) return;

        isPressed = false;
        UpdateSprite();
        offPress?.Invoke();
    }

    // 스프라이트 업데이트
    private void UpdateSprite()
    {
        buttonSR.sprite = isPressed ? pressed : notpressed;
    }
}