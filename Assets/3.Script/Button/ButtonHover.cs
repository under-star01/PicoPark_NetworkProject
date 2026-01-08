using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject Cursor;

    public void OnPointerEnter(PointerEventData e)
    {
        //Cursor.SetActive(true);
        Cursor.transform.position = transform.position;
    }

    public void OnPointerExit(PointerEventData e)
    {
        
    }
}
