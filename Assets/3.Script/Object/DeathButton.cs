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

    private bool isPressed = false;
    private List<PlayerMove> playersOnButton = new List<PlayerMove>();

    private void Awake()
    {
        buttonSR.sprite = notPressed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPressed) return; // 이미 눌렸으면 무시

        if (collision.CompareTag("Player"))
        {
            PlayerMove player = collision.GetComponentInParent<PlayerMove>();
            if (player != null && !playersOnButton.Contains(player))
            {
                playersOnButton.Add(player);
                Press();
            }
        }
    }

    private void Press()
    {
        if (isPressed) return;

        isPressed = true;
        buttonSR.sprite = pressed;

        Kill();
    }

    private void Kill()
    {
        foreach (PlayerMove player in playersOnButton)
        {
            if (player != null && player.isLocalPlayer)
            {
                CmdKill(player.netIdentity);
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdKill(NetworkIdentity playerNetId)
    {
        if (playerNetId == null) return;

        PlayerMove player = playerNetId.GetComponent<PlayerMove>();
        if (player != null)
        {
            RpcKill(playerNetId);
        }
    }

    [ClientRpc]
    private void RpcKill(NetworkIdentity playerNetId)
    {
        if (playerNetId == null) return;

        PlayerMove player = playerNetId.GetComponent<PlayerMove>();
        if (player != null)
        {
            player.Die();
        }
    }
}