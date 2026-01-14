using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dommy : MonoBehaviour
{
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private bool isGround = false;
    private Animator animator;

    private void Awake()
    {
        TryGetComponent(out animator);
        groundCheck = GetComponentInChildren<GroundCheck>();

        IgnoreSelfCollision();
    }
    private void Update()
    {
        isGround = groundCheck.IsGround;
        animator.SetBool("IsGround", isGround);
    }

    private void IgnoreSelfCollision()
    {
        Collider2D[] Cols = GetComponents<Collider2D>(); // 내 콜라이더
        Collider2D[] childCols = GetComponentsInChildren<Collider2D>(); //자식들 콜라이더

        //자식 콜라이더 갯수만큼 가져와서 무시하기
        foreach (var child in childCols)
        {
            if (child.transform == transform) continue; // 자기 자신 제외

            foreach (var parent in Cols)
            {
                Physics2D.IgnoreCollision(parent, child, true);
            }
        }
    }
}
