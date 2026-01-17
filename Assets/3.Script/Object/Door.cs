using Mirror;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [Header("문 스프라이트")]
    [SerializeField] private Sprite closedSprite; // 닫힌 문
    [SerializeField] private Sprite openedSprite; // 열린 문

    [Header("클리어 설정")]
    [SerializeField] private int totalPlayerCount = 2; // 총 플레이어 수
    private bool isStageCleared = false;

    [SyncVar(hook = nameof(OnDoorOpenedChanged))]
    private bool isOpened = false;

    private SpriteRenderer spriteRenderer;

    private HashSet<PlayerMove> enteredPlayers = new HashSet<PlayerMove>(); // 들어간 플레이어들
    private HashSet<PlayerMove> playersInRange = new HashSet<PlayerMove>(); // 문 범위 안 플레이어들

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 열쇠
        if (!isOpened && collision.CompareTag("Key"))
        {
            OpenDoorServer();
            NetworkServer.Destroy(collision.gameObject);
            return;
        }

        // 플레이어가 문 범위에 들어오면 리스트에 넣고
        if (collision.CompareTag("Player"))
        {
            PlayerMove player = collision.GetComponent<PlayerMove>();
            if (player != null)
                playersInRange.Add(player);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 플레이어가 문 범위에 나가면 리스트에서 나가
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMove player = collision.gameObject.GetComponent<PlayerMove>();
            if (player != null && playersInRange.Contains(player))
            {
                // 문 안에 들어가 있는 플레이어는 범위에서 제거하지 않음
                if (!enteredPlayers.Contains(player))
                {
                    playersInRange.Remove(player);
                }
            }
        }
    }

    [Server]
    private void OpenDoorServer()
    {
        isOpened = true;
    }

    private void OnDoorOpenedChanged(bool oldValue, bool newValue)
    {
        spriteRenderer.sprite = newValue ? openedSprite : closedSprite;
    }

    // 문 안으로 들어가기
    [Server]
    public void TryEnterDoor(PlayerMove player)
    {
        if (!isOpened) return;
        if (!playersInRange.Contains(player)) return;

        if (enteredPlayers.Contains(player))
        {
            ExitDoorServer(player);
        }
        else
        {
            EnterDoorServer(player);
        }
    }

    [Server]
    private void EnterDoorServer(PlayerMove player)
    {
        enteredPlayers.Add(player);

        player.SetMove(Vector2.zero);
        player.SetInsideDoor(true);

        RpcHidePlayer(player.netIdentity, true);

        if (enteredPlayers.Count >= totalPlayerCount)
        {
            StageClearServer();
        }
    }

    [Server]
    private void ExitDoorServer(PlayerMove player)
    {
        enteredPlayers.Remove(player);

        player.SetInsideDoor(false);
        RpcHidePlayer(player.netIdentity, false);
    }

    // 문 들어가면 숨기는 기능
    [ClientRpc]
    private void RpcHidePlayer(NetworkIdentity playerId, bool hide)
    {
        if (playerId == null) return;

        PlayerMove player = playerId.GetComponent<PlayerMove>();
        if (player == null) return;

        // 스프라이트 투명도
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = hide ? 0f : 1f;
            sr.color = c;
        }

        // 모자 활성화 / 비활성화
        PlayerCustom custom = player.GetComponent<PlayerCustom>();
        if (custom != null)
        {
            if (hide)
            {
                custom.HideHat();
            }
            else
            {
                custom.ActiveHat();
            }
        }

        // Collider 처리
        foreach (var col in player.GetComponentsInChildren<Collider2D>())
        {
            // Block 레이어만 제어
            if (col.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                col.enabled = !hide;
            }
        }

        // Rigidbody 안정화
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = hide ? 0f : 4f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    [Server]
    private void StageClearServer()
    {
        if (isStageCleared) return;
        isStageCleared = true;

        Debug.Log("스테이지 클리어!");

        // 여기서
        // - 모든 플레이어 입력 Lock
        // - 일정 시간 대기
        // - RpcFadeOut
        // - NetworkManager로 로비 씬 이동
    }
}