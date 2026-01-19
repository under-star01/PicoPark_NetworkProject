using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMetaData : MonoBehaviour
{
    // 연결 기준 정보
    public int connectionId;     // Mirror connectionId -> 서버 기준으로 유니크한 아니디
    public uint netId;           // NetworkIdentity.netId -> 생성 이후 확정

    // 게임 플레이용 정보
    public int playerIndex;      // Manager와 연결된 순서
    public int colorIndex;       // 색상 인덱스
    public int hatIndex;         // 모자 인덱스

    public PlayerMetaData(int connectionId, int playerIndex)
    {
        this.connectionId = connectionId;
        this.playerIndex = playerIndex;
        this.colorIndex = 0;
        this.hatIndex = 0;  
    }
}
