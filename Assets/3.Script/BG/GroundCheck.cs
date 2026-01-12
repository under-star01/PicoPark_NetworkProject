using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGround { get; private set; }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.isTrigger)
        {
            IsGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.isTrigger)
        {
            IsGround = false;
        }
    }
}
