using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class TitleMenuOnline : MonoBehaviour
{
    [Header("메뉴")]
    [SerializeField] GameObject[] menu; // StartGame, Return, FinishGame
    public int currentIndex = 0;

    // 옵션/종료 패널
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button OKButton;
    [SerializeField] private Button CancelButton;

    [SerializeField] private GameObject exitPanel;
    [SerializeField] private Button ExitOKButton;
    [SerializeField] private Button ExitCancelButton;

    [Header("Selection Colors")]
    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Color normalColor = Color.white;

    [Header("다음 씬")]
    // 전환용 씬 이름
    [SerializeField] private string nextSceneName = "NextScene";

    void Start()
    {
        menu[currentIndex].SetActive(true);
    }

    public void MoveRight()
    {
        menu[currentIndex].SetActive(false);
        currentIndex++;
        if (currentIndex >= menu.Length)
        {
            currentIndex = 0;
        }
        menu[currentIndex].SetActive(true);
    }

    public void MoveLeft()
    {
        menu[currentIndex].SetActive(false);
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = menu.Length - 1;
        }
        menu[currentIndex].SetActive(true);
    }

    // 각 항목 선택했을 때 작동할 메서드 연결
    public void Select()
    {
        switch (currentIndex)
        {
            case 0: Online(); break;
            //case 1: OpenOption(); break;
            case 2: OpenExit(); break;
        }
    }

    //위에서 연결될 메서드들
    void Online() // 온라인 버튼 눌렀으면 씬넘겨
    {
        SceneManager.LoadScene(nextSceneName);
    }


    public void OpenExit() // 종료 버튼 눌렀으면 종료 패널 켜
    {
        if (exitPanel != null) exitPanel.SetActive(true);
    }

    // 닫기/확인 버튼
    public void CloseOptionPanel()
    {
            optionPanel.SetActive(false); // 옵션창 꺼!
    }
    public void CloseExitNo()
    {
            exitPanel.SetActive(false); // 종료창 꺼!
    }
    public void ConfirmExitYes()
    {
        Application.Quit(); // 게임 꺼!
    }

    
}
