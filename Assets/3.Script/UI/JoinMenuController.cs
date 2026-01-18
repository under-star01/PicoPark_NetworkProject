using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class JoinMenuController : MonoBehaviour
{
    // 호스트 메뉴 항목
    [Header("UI References")]
    public Image hatImage;
    public Image colorImage;

    // 메뉴 항목의 value 초기화(기본값)
    [Header("Settings")]
    private int hatIndex = 0;
    private int colorIndex = 0;

    // 모자 및 플레이어 색 담을 상자
    [Header("Data")]
    public Sprite[] hatSprites; // 5개
    public Sprite[] playerColors; // 6개

    [SerializeField] private GameObject[] joinMenuButtons;
    [SerializeField] private TMP_InputField joinInputField;

    [SerializeField] private Button JoinRoomButton;
    [SerializeField] private Button JoinCancelButton;

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

    private void UpdateHatUI()
    {
        hatImage.sprite = hatSprites[hatIndex];
    }

    private void UpdateColorUI()
    {
        colorImage.sprite = playerColors[colorIndex];
    }

    public void ResetjoinInputField()
    {
        joinInputField.text = string.Empty;
    }

    // CANCEL 눌렀으면 그냥 꺼주기
    public void OnCancel()
    {
        gameObject.SetActive(false);
    }

    public void UpdateJoinPanelSelection(int panelIndex)
    {
        for(int i = 0; i < joinMenuButtons.Length; i++)
        {
            joinMenuButtons[panelIndex].GetComponent<ButtonHover>().OutFocus();
        }
        joinMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }

    public void FocusInputField()
    {
        joinInputField.Select();
        joinInputField.ActivateInputField();
    }

    public void InvokeJoin()
    {
        if (JoinRoomButton != null)
        {
            JoinRoomButton.onClick.Invoke();
        }
    }

    public void InvokeCancel()
    {
        if (JoinCancelButton != null)
        {
            JoinCancelButton.onClick.Invoke();
        }
    }


    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
