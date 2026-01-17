using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    [Header("플레이어 조작 방식")]
    [SerializeField] private bool isWASD;

    private IA_Player playerInput;
    private PlayerMove playerMove;
    private Door nearDoor; // 근처 문

    private void Awake()
    {
        playerInput = new IA_Player();
        TryGetComponent(out playerMove);

        playerInput.Disable();
    }

    // * Network Lifecycle

    public override void OnStartLocalPlayer()
    {
        EnableInput();
    }

    public override void OnStopLocalPlayer()
    {
        DisableInput();
    }

    // * Input Enable / Disable
    private void EnableInput()
    {
        if (isWASD)
        {
            playerInput.Player.Move.performed += Move;
            playerInput.Player.Move.canceled += Move;
            playerInput.Player.Jump.performed += JumpStart;
            playerInput.Player.Jump.canceled += JumpEnd;
        }
        else
        {
            playerInput.SubPlayer.Move.performed += Move;
            playerInput.SubPlayer.Move.canceled += Move;
            playerInput.SubPlayer.Jump.performed += JumpStart;
            playerInput.SubPlayer.Jump.canceled += JumpEnd;
        }

        playerInput.Enable();
    }

    private void DisableInput()
    {
        if (isWASD)
        {
            playerInput.Player.Move.performed -= Move;
            playerInput.Player.Move.canceled -= Move;
            playerInput.Player.Jump.performed -= JumpStart;
            playerInput.Player.Jump.canceled -= JumpEnd;
        }
        else
        {
            playerInput.SubPlayer.Move.performed -= Move;
            playerInput.SubPlayer.Move.canceled -= Move;
            playerInput.SubPlayer.Jump.performed -= JumpStart;
            playerInput.SubPlayer.Jump.canceled -= JumpEnd;
        }

        playerInput.Disable();
    }

    // * Input Callbacks (Client)

    private void Move(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;

        Vector2 input = context.ReadValue<Vector2>();
        CmdSetMove(input);
    }

    private void JumpStart(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        CmdJumpStart();
    }

    private void JumpEnd(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        CmdJumpEnd();
    }

    // * Commands (Client → Server)

    [Command]
    private void CmdSetMove(Vector2 input)
    {
        if (playerMove == null) return;
        playerMove.SetMove(input); // 서버에서 실행됨
    }

    [Command]
    private void CmdJumpStart()
    {
        if (playerMove == null) return;
        playerMove.JumpStart();
    }

    [Command]
    private void CmdJumpEnd()
    {
        if (playerMove == null) return;
        playerMove.JumpStop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 문 근처에 도착
        if (collision.gameObject.TryGetComponent(out Door door))
        {
            nearDoor = door;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 문에서 멀어짐
        if (collision.gameObject.TryGetComponent(out Door door))
        {
            if (nearDoor == door)
            {
                nearDoor = null;
            }
        }
    }
}
