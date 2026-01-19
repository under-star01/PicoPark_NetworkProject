using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class CustomNetMng : NetworkManager
{
    [Header("게임 상태 관리")]
    public bool isGameStarted = false; // 3번: 게임 진행 중 여부
    private List<NetworkConnectionToClient> waitingConnections = new List<NetworkConnectionToClient>();

    // 닉네임 관리 (서버 전용)
    public Dictionary<int, string> playerInfoMap = new Dictionary<int, string>();

    #region Server Side
    // 플레이어 입장 시
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (isGameStarted) // 3번: 게임 중이면 대기열로
        {
            waitingConnections.Add(conn);
            Debug.Log($"[Server] 게임 진행 중 - 접속자 {conn.connectionId} 대기열 추가");
            return;
        }

        base.OnServerAddPlayer(conn); // 플레이어 생성

        // 닉네임 설정 (DB 데이터가 있다면 여기서 할당)
        string newName = $"Player_{conn.connectionId}";
        playerInfoMap.Add(conn.connectionId, newName);

        Debug.Log($"[Server] {newName} 입장 완료");
    }

    // 플레이어 퇴장 시
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (playerInfoMap.TryGetValue(conn.connectionId, out string leaverName))
        {
            // 2번: 시스템 매니저를 통해 퇴장 알림 전송
            if (GameSystemManager.Instance != null)
                GameSystemManager.Instance.RpcNotifySystemMessage($"{leaverName} 님이 게임을 떠났습니다.");

            playerInfoMap.Remove(conn.connectionId);
        }

        // 대기열에 있던 유저라면 리스트에서 제거
        waitingConnections.Remove(conn);

        base.OnServerDisconnect(conn);
    }

    // 3번: 스테이지가 끝났을 때 대기 인원 소환 함수
    public void ReleaseWaitingPlayers()
    {
        isGameStarted = false;
        foreach (var conn in waitingConnections)
        {
            if (conn != null) base.OnServerAddPlayer(conn);
        }
        waitingConnections.Clear();

        if (GameSystemManager.Instance != null)
            GameSystemManager.Instance.RpcNotifySystemMessage("신규 플레이어가 합류했습니다!");
    }
    #endregion

    #region Client Side
    // 4번: 호스트(서버)가 끊겼을 때 클라이언트 처리
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        // 씬 이름이 OnlineMenu가 아니라면 (이미 메뉴가 아닌 게임 중이었다면)
        if (SceneManager.GetActiveScene().name != "OnlineMenu")
        {
            Debug.LogWarning("서버와의 연결이 끊겼습니다. 메뉴로 돌아갑니다.");
            SceneManager.LoadScene("OnlineMenu"); // 4번: 온라인 메뉴로 강제 이동
        }
    }
    #endregion
}