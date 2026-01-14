using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("플레이어 조작 방식")]
    [SerializeField] private bool isWASD;

    private IA_Player playerInput;
    private PlayerMove playerMove;

    private void Awake()
    {
        playerInput = new IA_Player();
        TryGetComponent(out playerMove);
    }

    private void OnEnable()
    {
        if (isWASD)
        {
            playerInput.Player.Move.performed += Move;
            playerInput.Player.Move.canceled += Move;
            playerInput.Player.Jump.performed += JumpStart;
            playerInput.Player.Jump.canceled += JumpEnd;
            playerInput.Enable();
        }
        else
        {
            playerInput.SubPlayer.Move.performed += Move;
            playerInput.SubPlayer.Move.canceled += Move;
            playerInput.SubPlayer.Jump.performed += JumpStart;
            playerInput.SubPlayer.Jump.canceled += JumpEnd;
            playerInput.Enable();
        }
    }

    private void OnDisable()
    {
        if (isWASD)
        {
            playerInput.Player.Move.performed -= Move;
            playerInput.Player.Move.canceled -= Move;
            playerInput.Player.Jump.performed -= JumpStart;
            playerInput.Player.Jump.canceled -= JumpEnd;
            playerInput.Disable();
        }
        else
        {
            playerInput.SubPlayer.Move.performed -= Move;
            playerInput.SubPlayer.Move.canceled -= Move;
            playerInput.SubPlayer.Jump.performed -= JumpStart;
            playerInput.SubPlayer.Jump.canceled -= JumpEnd;
            playerInput.Disable();
        }
    }

    private void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        playerMove.SetMove(input);
    }

    private void JumpStart(InputAction.CallbackContext context)
    {
        playerMove.JumpStart();
    }

    private void JumpEnd(InputAction.CallbackContext context)
    {
        playerMove.JumpStop();
    }
}
