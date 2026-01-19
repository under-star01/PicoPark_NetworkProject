using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DeathButton : NetworkBehaviour
{
    [Header("버튼 스프라이트")]
    [SerializeField] private SpriteRenderer buttonSR;
    [SerializeField] private Sprite pressed;
    [SerializeField] private Sprite notPressed;

    [SyncVar(hook = nameof(OnPressedChanged))]
    private bool isPressed = false;

    private void Awake()
    {
        buttonSR.sprite = notPressed;
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPressed) return;
        if (!collision.CompareTag("Player")) return;

        PressServer();
    }

    [Server]
    private void PressServer()
    {
        if (isPressed) return;

        AudioManager.Instance.PlaySFX("Button");

        isPressed = true;

        KillAllPlayersServer();
    }

    [Server]
    private void KillAllPlayersServer()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            PlayerMove player = conn.identity.GetComponent<PlayerMove>();
            if (player != null)
            {
                player.Die(); // 서버에서 사망 처리
            }
        }
    }

    private void OnPressedChanged(bool oldValue, bool newValue)
    {
        buttonSR.sprite = newValue ? pressed : notPressed;
    }
}