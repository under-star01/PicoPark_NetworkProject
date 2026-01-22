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
        private NetworkDriver m_Driver;
        private Unity.Networking.Transport.NetworkConnection m_ClientConn;
        private Dictionary<int, Unity.Networking.Transport.NetworkConnection> m_ServerConns = new Dictionary<int, Unity.Networking.Transport.NetworkConnection>();
        private RelayServerData m_RelayData;

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

        // [CS0506 �ذ�] Mirror ������� EarlyUpdate�� ����Ͽ� ��� ������ ��õ �����մϴ�.
        public override void ClientEarlyUpdate() => Tick();
        public override void ServerEarlyUpdate() => Tick();

        private void Tick()
        {
            if (!m_Driver.IsCreated) return;
            m_Driver.ScheduleUpdate().Complete();

            // 1. [Server ����] ���ο� ������ Accept()�� �����ɴϴ�. (image_119b86 ���� ����)
            if (NetworkServer.active)
            {
                Unity.Networking.Transport.NetworkConnection incoming;
                while ((incoming = m_Driver.Accept()) != default) // Accept�� ���ڸ� ���� �ʽ��ϴ�!
                {
                    int id = incoming.GetHashCode();
                    m_ServerConns[id] = incoming;
                    OnServerConnectedWithAddress?.Invoke(id, "RelayAddress");
                    Debug.Log($"[Server] �� Ŭ���̾�Ʈ ������. ID: {id}");
                }
            }

            // 2. [����] �̺�Ʈ ó�� (PopEvent)
            NetworkEvent.Type evt;
            // [CS1620 �ذ�] out Ű����� Ǯ���� Ÿ���� �����Ͽ� ������ �����ϴ�.
            while ((evt = m_Driver.PopEvent(out Unity.Networking.Transport.NetworkConnection conn, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        // Ŭ���̾�Ʈ ���忡���� ó�� (������ �� Accept���� �̹� ó����)
                        if (!NetworkServer.active) OnClientConnected?.Invoke();
                        break;

                    case NetworkEvent.Type.Data:
                        // ���⼭ ��Ŷ�� ���޵Ǿ�� �÷��̾� �������� ��ȯ�˴ϴ�.
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