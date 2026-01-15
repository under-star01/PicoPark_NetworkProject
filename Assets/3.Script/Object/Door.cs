using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("문 스프라이트")]
    [SerializeField] private Sprite closedSprite; // 닫힌 문
    [SerializeField] private Sprite openedSprite; // 열린 문

    [Header("클리어 설정")]
    [SerializeField] private int totalPlayerCount = 2; // 총 플레이어 수

    public bool isOpened = false;
    private SpriteRenderer spriteRenderer;
    private HashSet<PlayerMove> enteredPlayers = new HashSet<PlayerMove>(); // 들어간 플레이어들
    private List<PlayerMove> playersInRange = new List<PlayerMove>(); // 문 범위 안 플레이어들

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 열쇠
        if (collision.gameObject.CompareTag("Key"))
        {
            if (!isOpened)
            {
                OpenDoor(); //열어.
                Destroy(collision.gameObject); // 열쇠 제거
            }
        }

        // 플레이어가 문 범위에 들어오면 리스트에 넣고
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMove player = collision.gameObject.GetComponent<PlayerMove>();
            if (player != null && !playersInRange.Contains(player))
            {
                playersInRange.Add(player);
            }
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

    private void OpenDoor()
    {
        isOpened = true;
        spriteRenderer.sprite = openedSprite;
    }

    // 문 안으로 들어가기
    public void TryEnterDoor(PlayerMove player)
    {
        if (!isOpened) return; // 안열려있으면 나가

        // 이미 들어가 있으면 나와
        if (enteredPlayers.Contains(player))
        {
            ExitDoor(player);
            return; // 여기서 리턴
        }

        // 들어갈 때만 범위 체크(리스트에 없으면 나가)
        if (!playersInRange.Contains(player)) return;

        // 안들어가 있으면 드가
        EnterDoor(player);
    }

    //드가
    private void EnterDoor(PlayerMove player)
    {
        // 입력 초기화 (움직임 멈춤)
        player.SetMove(Vector2.zero);

        enteredPlayers.Add(player);
        player.SetInsideDoor(true); // 문 안 상태
        HidePlayer(player, true); // 숨기기

        Debug.Log($"{player.name} 문에 들어감. ({enteredPlayers.Count}/{totalPlayerCount})");

        // 모든 플레이어가 들어갔는지 확인
        if (enteredPlayers.Count >= totalPlayerCount)
        {
            StageClear();//다 들어가면 클리어
        }
    }

    //나가
    private void ExitDoor(PlayerMove player)
    {
        enteredPlayers.Remove(player);
        player.SetInsideDoor(false); // 문 밖 상태
        HidePlayer(player, false); // 숨겼던거 다시 보이기

        Debug.Log($"{player.name} 문에서 나옴. ({enteredPlayers.Count}/{totalPlayerCount})");
    }

    //문 들어가면 숨기는 기능
    private void HidePlayer(PlayerMove player, bool hide)
    {
        // 스프라이트만 투명하게
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = hide ? 0f : 1f; // 투명도 조절
            playerSprite.color = color;
        }

        // 콜라이더는 Trigger로 바꾸기 -> 충돌은 안하면서 나가는 입력 받을 수 있음
        Collider2D[] colliders = player.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (hide)
            {
                col.isTrigger = true; // 들어갈 때 isTrigger 켜
            }
            else
            {
                col.isTrigger = false; // 나오면 다시 꺼
            }
        }

        //Block 비활성화 or 활성화 -> 안하면 얘가 남아서 막음
        Collider2D[] childColliders = player.GetComponentsInChildren<Collider2D>();
        foreach (var col in childColliders)
        {
            if (col.CompareTag("Block"))
            {
                col.enabled = !hide;
            }
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (hide)
            {
                rb.gravityScale = 0f; //중력 끄고
                rb.linearVelocity = Vector2.zero; //멈춰
            }
            else
            {
                rb.gravityScale = 4f; // 중력 적용
            }
        }
    }

    private void StageClear()
    {
        Debug.Log("스테이지 클리어!");
        // SceneManager.LoadScene("select scene??");
    }

    // 문 리셋
    public void ResetDoor()
    {
        isOpened = false;
        spriteRenderer.sprite = closedSprite;

        // 들어간 플레이어들 다시 활성화
        foreach (var player in enteredPlayers)
        {
            if (player != null)
            {
                HidePlayer(player, false);
            }
        }

        enteredPlayers.Clear();
        playersInRange.Clear();
    }
}