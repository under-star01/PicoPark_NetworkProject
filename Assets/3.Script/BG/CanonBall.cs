using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBall : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator ani;
    private PlayerMove playerMove;

    private void OnEnable()
    {
        TryGetComponent(out rb);
        TryGetComponent(out ani);
    }
    private void FixedUpdate()
    {
        Shoot();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit();
    }

    private void Hit()
    {
        ani.SetTrigger("Hit");
    }

    private void Shoot()
    {
        
    }
}
