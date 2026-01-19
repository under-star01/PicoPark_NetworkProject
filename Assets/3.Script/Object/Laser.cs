using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    [Header("온오프")]
    [SerializeField] private bool onoff = false;
    [SerializeField] private float onDuration = 2f; //켜져있는 시간
    [SerializeField] private float offDuration = 2f; //꺼져있는 시간

    [Header("참조")]
    [SerializeField] private SpriteRenderer spriteR; //레이저 스프라이트
    [SerializeField] private Collider2D laserCollider; // 레이저 콜라이더

    [SyncVar(hook = nameof(OnChanged))] // 레이저 온오프를 클라이언트에 동기화
    private bool isLaserOn = true; // 레이저가 켜진 상태냐

    private Coroutine toggleCo; // 코루틴 참조 변수

    private void Start()
    {
        // 자동 참조 설정
        if (laserCollider == null)
            laserCollider = GetComponent<Collider2D>();

        // 초기 상태 동기화
        LaserState(isLaserOn);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (onoff)
        {
            toggleCo = StartCoroutine(OnOff_co());
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (toggleCo != null)
        {
            StopCoroutine(toggleCo);
        }
    }

    [Server]
    private IEnumerator OnOff_co()
    {
        while (true)
        {
            // 레이저 켜기
            isLaserOn = true;
            yield return new WaitForSeconds(onDuration);

            // 레이저 끄기
            isLaserOn = false;
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void OnChanged(bool oldValue, bool newValue)
    {
        LaserState(newValue);
    }

    private void LaserState(bool active)
    {
        // 콜라이더 활성화/비활성화
        if (laserCollider != null)
            laserCollider.enabled = active;

        // 비주얼 활성화/비활성화
        if (spriteR != null)
            spriteR.enabled = active;
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLaserOn) return; // 레이저가 꺼져있으면 나가
        if (!collision.CompareTag("Player")) return; //플레이어가 아니면 나가

        PlayerMove player = collision.GetComponent<PlayerMove>();
        if (player == null) return;

        player.Die(); // 서버에서만 사망 처리
    }
}