using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonOb : NetworkBehaviour
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

    [SyncVar(hook = nameof(OnPressedChanged))]
    private bool isPressed = false;

    private int playerCount = 0; // 버튼 위에 있는 플레이어 수

    public enum ButtonType
    {
        Toggle,  // 한번 누르면 작동
        Hold     // 누르는 동안만 작동
    }

    private void Awake()
    {
        if (buttonSR == null)
        {
            TryGetComponent(out buttonSR);
        }

        buttonSR.sprite = notpressed;
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerCount++;

        if (playerCount == 1)
        {
            PressServer();
        }
    }

    [ServerCallback]
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerCount--;
        playerCount = Mathf.Max(0, playerCount);

        if (playerCount == 0 && buttonType == ButtonType.Hold)
        {
            ReleaseServer();
        }
    }

    [Server]
    private void PressServer()
    {
        if (buttonType == ButtonType.Toggle && isPressed) return;

        NetworkAudio.Instance.PlaySharedSFX("Button");

        isPressed = true;
        onPress?.Invoke();
    }

    [Server]
    private void ReleaseServer()
    {
        if (!isPressed) return;

        isPressed = false;
        offPress?.Invoke();
    }

    // 스프라이트 업데이트
    private void OnPressedChanged(bool oldValue, bool newValue)
    {
        buttonSR.sprite = newValue ? pressed : notpressed;
    }
}