using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    private int currentIndex = 0;
    [SerializeField] GameObject[] menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menu[currentIndex].SetActive(true);
    }

    // Update is called once per frame
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
