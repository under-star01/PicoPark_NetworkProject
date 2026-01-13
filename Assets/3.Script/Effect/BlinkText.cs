using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private float blinkSpeed = 1f;

    // Update is called once per frame
    void OnEnable()
    {
        StartCoroutine(Blink_Co());
    }

    private IEnumerator Blink_Co()
    {
        while (true)
        {
            Color color = text.color;
            color.a = 1f;
            text.color = color;

            yield return new WaitForSeconds(blinkSpeed);

            color.a = 0f;
            text.color = color;

            yield return new WaitForSeconds(blinkSpeed);
        }
        
    }
}
