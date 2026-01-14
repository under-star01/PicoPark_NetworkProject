using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
}
