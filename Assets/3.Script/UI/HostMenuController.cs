using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Mirror;

public class HostMenuController : MonoBehaviour
{
    // 호스트 메뉴 항목
    [Header("UI References")]
    public Image maxPlayerText;
    public Image joinProgressText;
    public Image hatImage;
    public Image colorImage;

    // 메뉴 항목의 value 초기화(기본값)
    [Header("Settings")]
    private int maxPlayers = 2;
    private bool joinInProgress = true;
    private int hatIndex = 0;
    private int colorIndex = 0;

    // 모자 및 플레이어 색 담을 상자
    [Header("Data")]
    public Sprite[] numberSprites; // 2,3,4,5,6 순서대로 5개
    public Sprite yesSprite;
    public Sprite noSprite;
    public Sprite[] hatSprites; // 5개
    public Sprite[] playerColors; // 6개

    // 플레이어 최소 최댓값 설정
    private const int MIN_PLAYERS = 2;
    private const int MAX_PLAYERS = 6;

    [SerializeField] private GameObject[] HostMenuButtons;
 


    // MAX PLAYER수 증감 및 적용
    public void OnMaxPlayerLeft()
    {
        if (maxPlayers > MIN_PLAYERS)
        {
            maxPlayers--;
            UpdateMaxPlayerUI();
        }
    }

    public void OnMaxPlayerRight()
    {
        if (maxPlayers < MAX_PLAYERS)
        {
            maxPlayers++;
            UpdateMaxPlayerUI();
        }
    }

    // 중도참여 상태 버튼에 따라 전환해주기(참/거짓)
    public void OnJoinProgress()
    {
        joinInProgress = !joinInProgress;
        UpdateJoinProgressUI();
    }

    // 모자 이미지 전환(배열의 인덱스 순서대로 증감)
    public void OnHatLeft()
    {
        hatIndex = (hatIndex - 1 + hatSprites.Length) % hatSprites.Length;
        UpdateHatUI();
    }

    public void OnHatRight()
    {
        hatIndex = (hatIndex + 1) % hatSprites.Length;
        UpdateHatUI();
    }

    // 플레이어 색 전환(배열 인덱스 증감)
    public void OnColorLeft()
    {
        colorIndex = (colorIndex - 1 + playerColors.Length) % playerColors.Length;
        UpdateColorUI();
    }

    public void OnColorRight()
    {
        colorIndex = (colorIndex + 1) % playerColors.Length;
        UpdateColorUI();
    }

    //상단에서 변경한 내용을 UI에 적용시킴
    private void UpdateMaxPlayerUI()
    {
        maxPlayerText.sprite = numberSprites[maxPlayers - 2];
    }

    private void UpdateJoinProgressUI()
    {
        joinProgressText.sprite = joinInProgress ? yesSprite : noSprite;
    }

    private void UpdateHatUI()
    {
        hatImage.sprite = hatSprites[hatIndex];
    }

    private void UpdateColorUI()
    {
        colorImage.sprite = playerColors[colorIndex];
    }


    // CANCEL 눌렀으면 그냥 꺼주기
    public void OnCancel()
    {
        Debug.Log("종료버튼 눌렸다!");
        gameObject.SetActive(false);
    }

    // Create 버튼 -> 현재 선택 내용 GameFlowManager에 저장
    public void OnClickCreate()
    {
        int selectedHat = hatIndex;
        int selectedColor = colorIndex;

        CmdSubmitPlayerMeta(selectedColor, selectedHat);
    }

    public void UpdateHostPanelSelection(int panelIndex)
    {
        for(int i = 0; i < HostMenuButtons.Length; i++)
        {
            HostMenuButtons[panelIndex].GetComponent<ButtonHover>().OutFocus();
        }
        HostMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }
}
