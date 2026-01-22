using Mirror;
using System.Collections;
using UnityEngine;

public class TrafficLight : NetworkBehaviour
{
    [Header("신호등 스프라이트")]
    [SerializeField] private Sprite redSprite;      // 빨간불
    [SerializeField] private Sprite blueSprite;    // 파란불
    [SerializeField] private Sprite offSprite;      // 꺼진 불

    [Header("타이밍 설정")]
    [SerializeField] private float redDuration = 5f;        // 빨간불 지속 시간
    [SerializeField] private float blueDuration = 5f;      // 파란불 지속 시간
    [SerializeField] private float blinkDuration = 2f;      // 깜빡이는 시간
    [SerializeField] private float blinkTerm = 0.3f;    // 깜빡이는 간격

    [Header("참조")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SyncVar(hook = nameof(LightChanged))]
    private LightState currentState = LightState.blue;

    [SyncVar(hook = nameof(SpriteChanged))]
    private int currentSprite = 1; // 0: Red, 1: blue, 2: Off

    private Coroutine lightCycle;

    private enum LightState
    {
        Red,    // 빨간불 (움직이면 죽음)
        blue   // 파란불 (안전)
    }

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateSprite();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        lightCycle = StartCoroutine(LightCycle_co());
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (lightCycle != null)
            StopCoroutine(lightCycle);
    }

    [Server]
    private IEnumerator LightCycle_co()
    {
        while (true)
        {
            // 파란불
            currentState = LightState.blue;
            currentSprite = 1; // blue
            yield return new WaitForSeconds(blueDuration - blinkDuration);

            // 파란불 깜빡임
            yield return StartCoroutine(Blink_co(1)); // 1 = blue

            // 빨간불
            currentState = LightState.Red;
            currentSprite = 0; // Red
            yield return new WaitForSeconds(redDuration - blinkDuration);

            // 빨간불 깜빡임
            yield return StartCoroutine(Blink_co(0)); // 0 = Red
        }
    }

    [Server]
    private IEnumerator Blink_co(int targetIndex)
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            // 원래 불
            currentSprite = targetIndex;
            yield return new WaitForSeconds(blinkTerm);

            // 꺼진 불
            currentSprite = 2; // Off
            yield return new WaitForSeconds(blinkTerm);

            elapsed += blinkTerm * 2;
        }
    }

    private void LightChanged(LightState oldState, LightState newState)
    {
        // 상태 변경 시 추가 로직 (필요시)
    }

    private void SpriteChanged(int oldIndex, int newIndex)
    {
        SpriteIndex(newIndex);
    }

    private void UpdateSprite()
    {
        SpriteIndex(currentSprite);
    }

    private void SpriteIndex(int index)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.sprite = index switch
        {
            0 => redSprite,
            1 => blueSprite,
            2 => offSprite,
            _ => offSprite
        };
    }

    // 플레이어가 범위 안에서 움직이는지 체크
    [ServerCallback]
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentState != LightState.Red) return;
        if (!collision.CompareTag("Player")) return;

        PlayerMove player = collision.GetComponent<PlayerMove>();
        if (player == null) return;

        // 플레이어가 움직이고 있으면 죽음
        if (player.IsMoving())
        {
            player.Die();
        }
    }
}