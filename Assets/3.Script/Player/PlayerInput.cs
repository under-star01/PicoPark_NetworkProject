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

    public IA_Player playerInput;
    private PlayerMove playerMove;
    private Door nearDoor; // 근처 문

    private void Awake()
    {
        playerInput = new IA_Player();
        TryGetComponent(out playerMove);
    }

    public IA_Player GetPlayerInput() => playerInput;

    public override void OnStartLocalPlayer()
    {

        if (playerInput == null) playerInput = new IA_Player();
        playerInput.Enable();

        var uiManager = FindFirstObjectByType<OnlineMenu_UIManager>();
        if (uiManager != null)
        {
            uiManager.RegisterLocalPlayer(this);
        }

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
        if (isLocalPlayer)
        {
            playerInput.Disable();
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

    private void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        playerMove.SetMove(input);
    }

    private void JumpStart(InputAction.CallbackContext context)
    {
        // 근처에 열린 문이 있으면 문 입장/퇴장
        if (nearDoor != null && nearDoor.isOpened)
        {
            nearDoor.TryEnterDoor(playerMove);
        }
        else
        {
            // 문이 없으면 점프
            playerMove.JumpStart();
        }
    }

    private void JumpEnd(InputAction.CallbackContext context)
    {
        playerMove.JumpStop();
    }
}
