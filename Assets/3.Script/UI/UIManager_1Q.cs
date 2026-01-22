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
    public void MoveToStageSelect()
    {
        // 오직 호스트(서버)만 씬 전환 및 상태 변경 권한이 있습니다.
        if (NetworkServer.active)
        {
            // 안전한 형변환(as)을 사용합니다.
            CustomNetMng manager = NetworkManager.singleton as CustomNetMng;

            if (manager != null)
            {
                // 1. 스테이지 선택창은 '준비 단계'이므로 false로 설정
                // 이렇게 해야 새로 들어오는 유저가 대기열에 안 빠지고 바로 캐릭터가 생깁니다.
                //manager.isGameStarted = false;

                Debug.Log("스테이지 셀렉트로 이동: 이제 신규 유저는 즉시 소환됩니다.");
                manager.ServerChangeScene("Scene_4.StageSelect");
            }
        }
    }

    public void MoveToStage()
    {
        if (NetworkServer.active)
        {
            CustomNetMng manager = NetworkManager.singleton as CustomNetMng;

            if (manager != null)
            {
                // 2. 실제 게임 스테이지는 '진행 중'이므로 true로 설정
                // 이제부터 접속하는 유저는 CustomNetMng의 OnServerAddPlayer에 의해 대기열로 갑니다.
                //manager.isGameStarted = true;

                Debug.Log("게임 스테이지로 이동: 이제 신규 유저는 대기열(Queue)로 이동합니다.");
                manager.ServerChangeScene("Scene_MintNyang");
            }
        }
    }

}
