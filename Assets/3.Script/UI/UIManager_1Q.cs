using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager_1Q : MonoBehaviour
{
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject hostMenuUI;
    [SerializeField] GameObject clientMenuUI;

    public void PressHostBtn()
    {
        Debug.Log("호스트 버튼 누름");
        mainMenuUI.SetActive(false);
        hostMenuUI.SetActive(true);
        clientMenuUI.SetActive(false);
    }

    public void PressClientBtn()
    {
        Debug.Log("클라이언트 버튼 누름");
        mainMenuUI.SetActive(false);
        hostMenuUI.SetActive(false);
        clientMenuUI.SetActive(true);
    }

    public void PressCreateBtn()
    {
        Debug.Log("방 생성 버튼 누름");
        hostMenuUI.SetActive(false);
    }

    public void PressJoinBtn()
    {
        Debug.Log("참여 버튼 누름");
        clientMenuUI.SetActive(false);
    }

    public void StartGame()
    {
        // 1. 서버(호스트)인지 확인 (클라이언트는 이 값을 바꿀 권한이 없음)
        if (!NetworkServer.active)
        {
            Debug.LogWarning("호스트만 게임을 시작할 수 있습니다.");
            return;
        }

        // 2. 현재 네트워크 매니저를 CustomNetMng로 형변환하여 가져옴
        CustomNetMng manager = (CustomNetMng)NetworkManager.singleton;

        if (manager != null)
        {
            // 3. 게임 시작 상태로 변경
            manager.isGameStarted = true;
            Debug.Log("게임 시작! 이제부터 접속하는 유저는 대기열로 이동합니다.");

            // 4. (선택 사항) 실제 인게임 스테이지 씬으로 이동
            // manager.ServerChangeScene("InGameStage01");
        }
    }
}
