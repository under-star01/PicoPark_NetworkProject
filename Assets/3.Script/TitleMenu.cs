using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class TitleMenu : MonoBehaviour
{
    public int currentIndex = 0;
    [SerializeField] GameObject[] menu;

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
}
