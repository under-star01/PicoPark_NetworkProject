using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetwork : NetworkBehaviour
{
    private PlayerMove move;

    //private void Awake()
    //{
    //    move = GetComponent<PlayerMove>();
    //}

    //public void SendMove(Vector2 input)
    //{
    //    if (!hasAuthority) return;
    //    CmdMove(input);
    //}

    //[Command]
    //private void CmdMove(Vector2 input)
    //{
    //    move.ServerMove(input);
    //}

    //public void SendJump()
    //{
    //    if (!hasAuthority) return;
    //    CmdJump();
    //}

    //[Command]
    //private void CmdJump()
    //{
    //    move.ServerJump();
    //}
}
