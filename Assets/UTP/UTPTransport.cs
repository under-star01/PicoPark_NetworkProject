using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;

namespace Mirror.Transports.Utp
{
    public class UTPTransport : Transport
    {
        private NetworkDriver m_Driver; //통신의 핵심엔진(실제 데이터를 쏘고 받는 역할)
        private Unity.Networking.Transport.NetworkConnection m_ClientConn;

        //서버가 접속한 여려명의 클라이언트 명단을 Mirror 형식에 맞게 배열에 저장(Mirror는 정수(int) 형태 아이디만 받음)
        private Dictionary<int, Unity.Networking.Transport.NetworkConnection> m_ServerConns 
            = new Dictionary<int, Unity.Networking.Transport.NetworkConnection>();
        private RelayServerData m_RelayData;

        //Override를 통해 Transport의 정의된 틀을 재정의함
        public void SetRelayServerData(RelayServerData data) => m_RelayData = data;
        public override bool Available() => Application.platform != RuntimePlatform.WebGLPlayer;
        public override Uri ServerUri() => new Uri("utp://127.0.0.1");

        public override bool ClientConnected() => m_Driver.IsCreated && m_ClientConn.IsCreated &&
            m_Driver.GetConnectionState(m_ClientConn) == Unity.Networking.Transport.NetworkConnection.State.Connected;

        public override void ClientConnect(string address)
        {
            var settings = new NetworkSettings();
            settings.WithRelayParameters(ref m_RelayData);
            m_Driver = NetworkDriver.Create(settings);
            m_ClientConn = m_Driver.Connect();
        }

        public override void ClientDisconnect() => Shutdown();

        public override void ClientSend(ArraySegment<byte> segment, int channelId)
        {
            if (!ClientConnected()) return;
            m_Driver.BeginSend(m_ClientConn, out var writer);
            writer.WriteBytes(new NativeArray<byte>(segment.Array, Allocator.Temp).GetSubArray(segment.Offset, segment.Count));
            m_Driver.EndSend(writer);
        }

        public override void ServerStart()
        {
            var settings = new NetworkSettings();
            settings.WithRelayParameters(ref m_RelayData);
            m_Driver = NetworkDriver.Create(settings);
            if (m_Driver.Bind(NetworkEndpoint.AnyIpv4) == 0) m_Driver.Listen();
        }

        public override void ServerStop() => Shutdown();
        public override bool ServerActive() => m_Driver.IsCreated && m_Driver.Listening;

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
        {
            if (m_ServerConns.TryGetValue(connectionId, out var conn) && m_Driver.GetConnectionState(conn) == Unity.Networking.Transport.NetworkConnection.State.Connected)
            {
                m_Driver.BeginSend(conn, out var writer);
                writer.WriteBytes(new NativeArray<byte>(segment.Array, Allocator.Temp).GetSubArray(segment.Offset, segment.Count));
                m_Driver.EndSend(writer);
            }
        }

        public override void ServerDisconnect(int connectionId) => Shutdown();
        public override string ServerGetClientAddress(int connectionId) => "RelayAddress";
        public override void Shutdown() { if (m_Driver.IsCreated) m_Driver.Dispose(); m_ServerConns.Clear(); }
        public override int GetMaxPacketSize(int channelId = 0) => 1200;

        // [CS0506 해결] Mirror 정석대로 EarlyUpdate를 사용하여 상속 에러를 원천 차단합니다.
        public override void ClientEarlyUpdate() => Tick();
        public override void ServerEarlyUpdate() => Tick();

        private void Tick()
        {
            if (!m_Driver.IsCreated) return;
            m_Driver.ScheduleUpdate().Complete();

            // 1. [Server 전용] 새로운 연결을 Accept()로 가져옵니다. (image_119b86 로직 적용)
            if (NetworkServer.active)
            {
                Unity.Networking.Transport.NetworkConnection incoming;
                while ((incoming = m_Driver.Accept()) != default) // Accept는 인자를 받지 않습니다!
                {
                    int id = incoming.GetHashCode();
                    m_ServerConns[id] = incoming;
                    OnServerConnectedWithAddress?.Invoke(id, "RelayAddress");
                    Debug.Log($"[Server] 새 클라이언트 수락됨. ID: {id}");
                }
            }

            // 2. [공통] 이벤트 처리 (PopEvent)
            NetworkEvent.Type evt;
            // [CS1620 해결] out 키워드와 풀네임 타입을 명시하여 에러를 막습니다.
            while ((evt = m_Driver.PopEvent(out Unity.Networking.Transport.NetworkConnection conn, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        // 클라이언트 입장에서만 처리 (서버는 위 Accept에서 이미 처리됨)
                        if (!NetworkServer.active) OnClientConnected?.Invoke();
                        break;

                    case NetworkEvent.Type.Data:
                        // 여기서 패킷이 전달되어야 플레이어 프리팹이 소환됩니다.
                        byte[] data = new byte[stream.Length];
                        stream.ReadBytes(data);
                        if (NetworkServer.active) OnServerDataReceived?.Invoke(conn.GetHashCode(), new ArraySegment<byte>(data), 0);
                        else OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), 0);
                        break;

                    case NetworkEvent.Type.Disconnect:
                        if (NetworkServer.active) OnServerDisconnected?.Invoke(conn.GetHashCode());
                        else OnClientDisconnected?.Invoke();
                        break;
                }
            }
        }
    }
}