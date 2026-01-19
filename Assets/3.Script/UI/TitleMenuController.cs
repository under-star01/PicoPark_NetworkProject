using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Mirror;
using TMPro;

public class TitleMenuController : MonoBehaviour
{
    [Header("메뉴")]
    [SerializeField] private GameObject window;
    [SerializeField] private GameObject[] titlePanels; // StartGame, Return, FinishGame
    public int currentIndex = 0;

    [Header("Selection Colors")]
    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Press Button")]
    [SerializeField] public GameObject pressbutton;

    [Header("Head")]
    [SerializeField] private GameObject Head;
    [SerializeField] private TMP_Text HeadCount;

    [Header("다음 씬")]
    // 전환용 씬 이름
    [SerializeField] private string nextSceneName = "NextScene";


    void Start()
    {
        titlePanels[currentIndex].SetActive(true);
    }

    public void MoveRight()
    {
        titlePanels[currentIndex].SetActive(false);
        currentIndex++;

        if (currentIndex >= titlePanels.Length) currentIndex = 0;

        // 다음 칸이 2인데 2명이 안 되면
        if (currentIndex == 2 && !CanStartGameCheck())
        {
            currentIndex = 0;
        }

        titlePanels[currentIndex].SetActive(true);
    }

    public void MoveLeft()
    {
        titlePanels[currentIndex].SetActive(false);
        currentIndex--;

        if (currentIndex < 0) currentIndex = titlePanels.Length - 1;
        // 이전 칸이 2인데 2명이 안 되면
        if (currentIndex == 2 && !CanStartGameCheck())
        {
            currentIndex = 1;
        }

        titlePanels[currentIndex].SetActive(true);
    }

    private bool CanStartGameCheck()
    {
        int playerCount = NetworkManager.singleton.numPlayers;
        // 호스트(서버)이면서 플레이어가 2명 이상일 때만 True
        return NetworkServer.active && playerCount >= 2;
    }


    //위에서 연결될 메서드들
    void Online() // 온라인 버튼 눌렀으면 씬넘겨
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void ConfirmExitYes()
    {
        Application.Quit(); // 게임 꺼!
    }

    public void SetActive(bool isActive)
    {
        if (window != null)
        {
            window.SetActive(isActive);
        }
        if (isActive)
        {
            titlePanels[currentIndex].SetActive(true);
        }
        else
        {
            HideAllPanels();
        }
    }

    public void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void SetPressButtonActive(bool active)
    {
        if (pressbutton != null)
        {
            pressbutton.SetActive(active);
        }
    }

    public void InitTitleMenu()
    {
        // 모든 패널을 비활성화
        if (window != null) window.SetActive(true);
        HideAllPanels();

        // 조건에 따라 인덱스 설정
        if (CanStartGameCheck())
        {
            currentIndex = 2; // 바로 시작
        }
        else
        {
            currentIndex = 0; // 조건이 안 되면 0 표시
        }

        titlePanels[currentIndex].SetActive(true);
    }

    public void HideAllPanels()
    {
        foreach (GameObject panel in titlePanels)
        {
            if (panel != null) panel.SetActive(false);
        }

    }

    public bool IsUIActive()
    {
        return window != null && window.activeSelf;
    }

    public void SetHeadActive()
    {
        if (Head == null || HeadCount == null) return;

        Head.SetActive(true);

        int current = NetworkManager.singleton.numPlayers;
        int max = NetworkManager.singleton.maxConnections;

        HeadCount.text = $"{current} / {max}";
    }

    public void SetheadCountText(string headCount)
    {
        HeadCount.text = headCount;
    }

    public void ExecuteSelection()
    {
        switch (currentIndex)
        {
            case 0: // Return (뒤로가기/패널 끄기)
                SetActive(false);
                SetPressButtonActive(true);
                OnlineMenu_UIManager.Instance.RestorePlayerMode();
                break;

            case 1: // Finish Game (타이틀/로비로 나가기)
                Debug.Log("게임 타이틀로 이동");
                // Mirror 사용 중이라면 NetworkManager를 통해 연결을 끊고 씬 이동
                if (NetworkServer.active && NetworkClient.isConnected)
                {
                    NetworkManager.singleton.StopHost();
                }
                else
                {
                    NetworkManager.singleton.StopClient();
                }
                SceneManager.LoadScene("Scene_1.Title");
                break;

            case 2: // Start Game (게임 시작)
                Debug.Log("게임 시작!");
                // 호스트일 때만 씬을 전환 (Server Change Scene)
                if (NetworkServer.active)
                {
                    OnlineMenu_UIManager.Instance.changeState(3);
                    //NetworkManager.singleton.ServerChangeScene("InGameSceneName");
                }
                break;
        }
    }
}
