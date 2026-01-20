using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay; // 필수 네임스페이스
using UnityEngine;
using TMPro;
using Mirror.Transports.Utp;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinInputField;
    [SerializeField] private GameObject Log;
    [SerializeField] private TMP_Text hostID;
    private UTPTransport transport;

    async void Start()
    {
        if (NetworkManager.singleton != null)
        {
            transport = NetworkManager.singleton.GetComponent<UTPTransport>();
        }
        Log.SetActive(false);
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelayHost(int maxPlayers = 6)
    {
        if (transport == null)
            transport = NetworkManager.singleton.GetComponent<Mirror.Transports.Utp.UTPTransport>();

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"방 생성 성공! 코드: {code}");
            hostID.text = string.Format("ID : {0}", code);

            // [교정된 순서] 반드시 이 순서를 지켜야 합니다.
            // 1. Host, 2. Port, 3. AllocationId, 4. ConnectionData, 5. HostConnectionData, 6. Key, 7. IsSecure
            var relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,       // 4번: 본인의 연결 데이터
                allocation.ConnectionData,       // 5번: 호스트로서의 연결 데이터 (호스트는 본인 것 사용)
                allocation.Key,                  // 6번: HMAC 키 (반드시 64바이트여야 함)
                false                            // 7번: 보안 연결 여부
            );

            transport.SetRelayServerData(relayServerData);
            NetworkManager.singleton.StartHost();
            OnlineMenu_UIManager.Instance.changeState(2);

        }
        catch (RelayServiceException e) { Debug.LogError(e.Message); }
    }

    public void OnClickJoinButton() => JoinRelayServer(joinInputField.text);

    public async void JoinRelayServer(string inputCode)
    {
        if (string.IsNullOrWhiteSpace(inputCode))
        {
            Log.SetActive(true);
            Log.GetComponent<TMP_Text>().text = "접속 코드가 비어있습니다!\n코드를 입력해주세요.";
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputCode);

            // [교정된 순서] 클라이언트 버전
            var relayServerData = new RelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,     // 4번: 본인의 연결 데이터
                joinAllocation.HostConnectionData, // 5번: 접속하려는 호스트의 연결 데이터
                joinAllocation.Key,                // 6번: HMAC 키
                false
            );

            transport.SetRelayServerData(relayServerData);
            NetworkManager.singleton.StartClient();
            OnlineMenu_UIManager.Instance.changeState(2);
        }
        catch (RelayServiceException e)
        {
            Log.SetActive(true);
            Log.GetComponent<TMP_Text>().text = "입력하신 ROOM CODE는\n존재하지 않습니다.";
        }
    }
}