using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyStatusManager : NetworkBehaviour
{
    // [SyncVar]에 Hook을 사용하면 값이 바뀔 때마다 모든 클라이언트에서 특정 함수를 실행합니다.
    [SyncVar(hook = nameof(OnPlayerCountChanged))]
    private int playerCount = 0;

    [SyncVar(hook = nameof(OnMaxPlayerChanged))]
    private int maxPlayer = 0;

    [SerializeField] private TitleMenuController titleMenuController;

    // 서버에서만 실행되는 인원수 업데이트 함수
    [Server]
    public void UpdateStatus(int current, int max)
    {
        playerCount = current;
        maxPlayer = max;
    }

    // 값이 변할 때 클라이언트에서 자동으로 실행될 함수
    void OnPlayerCountChanged(int oldVal, int newVal) => UpdateUI();
    void OnMaxPlayerChanged(int oldVal, int newVal) => UpdateUI();

    void UpdateUI()
    {
        // 아까 사용하신 UI 갱신 코드 호출
        // 인스펙터나 싱글톤을 통해 TitleMenuController를 참조하세요.
        string text = string.Format("{0}/{1}", playerCount, maxPlayer);
        titleMenuController.SetheadCountText(text);
    }
}
