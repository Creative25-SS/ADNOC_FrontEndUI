using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

using EasyTcp4;
using EasyTcp4.ServerUtils;
using EasyTcp4.ClientUtils;
using EasyTcp4.ClientUtils.Async;

using UTool.Utility;

using DG.Tweening;

namespace UTool.Networking
{
    public class TCPHandler
    {
        private string ip;
        private int port;

        private EasyTcpServer tcpServer;
        private EasyTcpClient tcpClient;

        private bool connectedToServer => tcpClient == null ? false : tcpClient.IsConnected();

        private bool connecting = false;
        private Tween reConnectionTween;

        public bool serverRunning = false;
        public bool log = false;

        public UnityEvent<string, bool> OnConnectionToServer = new UnityEvent<string, bool>();
        public UnityEvent<string, bool> OnClientConnection = new UnityEvent<string, bool>();
        public UnityEvent<string, byte[]> OnDataReceived = new UnityEvent<string, byte[]>();

        public Dictionary<string, List<byte>> accumulatingData = new Dictionary<string, List<byte>>();

        private bool reConnectToServer = false;

        public void SetEndPoint(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Tick()
        {
            if (reConnectToServer)
            {
                reConnectToServer = false;

                StopService();
                ConnectToServer();
            }

            //if (!connecting)
            //    if (tcpClient != null)
            //        Debug.Log(connectedToServer);

            //if (tcpServer != null)
            //    Debug.Log(tcpServer.ConnectedClientsCount);
        }

        #region TCP Server

        public void StartServer()
        {
            if (serverRunning)
            {
                ServerLog("Server Already Running");
                return;
            }

            tcpServer = new EasyTcpServer();

            tcpServer.OnConnect += (o, client) => ClientConnection(connected: true, client);
            tcpServer.OnDisconnect += (o, client) => ClientConnection(connected: false, client);

            tcpServer.OnDataReceive += OnDataReceive;

            tcpServer.OnError += (o, e) => ServerLog(e.ToString(), 2);

            Task.Run(() =>
            {
                tcpServer.Start(ip, (ushort)port);
            });

            serverRunning = true;

            ServerLog("Server Started");
        }

        private void ClientConnection(bool connected, EasyTcpClient client)
        {
            IPEndPoint clientAddress = client.GetEndPoint();
            string ipPort = $"{clientAddress.Address}:{clientAddress.Port}";

            ServerLog($"Client '{ipPort}' {(connected ? "Connected" : "Disconnected")}");

            OnClientConnection?.Invoke(ipPort, connected);
        }

        public void SendClient(string ipPort, byte[] data)
        {
            EasyTcpClient clientToSend = tcpServer.GetConnectedClients().Find(x => SameIPAddress(x.GetEndPoint(), ipPort));

            if (clientToSend == null)
            {
                ServerLog($"Unable to Find Client With ID '{ipPort}'", 1);
                return;
            }

            Task.Run(() =>
            {
                clientToSend.SendArray(data);
            });

            ServerLog($"Sending Data To Client : {ipPort}");
        }

        #endregion

        #region TCP Client

        public void ConnectToServer()
        {
            if (connectedToServer)
            {
                ClientLog("Already Connected to Server");
                return;
            }

            tcpClient = new EasyTcpClient();

            tcpClient.OnConnect += (o, client) => ServerConnection(connected: true, client);
            tcpClient.OnDisconnect += (o, client) => ServerConnection(connected: false, client);

            tcpClient.OnDataReceive += OnDataReceive;

            tcpClient.OnError += (o, e) => ClientLog(e.ToString(), 2); ;

            ClientLog($"Connecting To Server '{ip}:{port}'");

            TryConnectingToServer();
        }

        private async void TryConnectingToServer()
        {
            if (connecting)
                return;

            connecting = true;
            bool connected = await tcpClient.ConnectAsync(ip, (ushort)port);
            connecting = false;

            if (!connected)
            {
                ClientLog("Failed To Connect to Server : Retrying in 1 Seconds");

                UUtility.KillTween(reConnectionTween);
                reConnectionTween = DOVirtual.DelayedCall(1, () =>
                {
                    ClientLog("Re-Connecting To Server");
                    reConnectToServer = true;
                });
            }
        }

        private IPEndPoint GetIPEndPoint()
        {
            IPAddress.TryParse(ip, out IPAddress address);
            return new IPEndPoint(address, port);
        }

        private void ServerConnection(bool connected, EasyTcpClient server)
        {
            IPEndPoint serverAddress = server != null ? server.GetEndPoint() : GetIPEndPoint();
            string ipPort = $"{serverAddress.Address}:{serverAddress.Port}";

            ClientLog($"{(connected ? "Connected To" : "Disconnected From")} Server '{ipPort}'");

            if (!connected)
            {
                //if (log)
                //    Debug.Log($"[TCP Client] - Retrying Connection To Server '{ipPort}'");

                connecting = false;
                reConnectToServer = true;
            }

            OnConnectionToServer?.Invoke(ipPort, connected);
        }

        public void Send(byte[] data)
        {
            Task.Run(() =>
            {
                tcpClient.SendArray(data);
            });

            ClientLog($"Sending Data");
        }

        public void SendAll(byte[] data)
        {
            try
            {
                tcpServer.SendAll(data);
            }
            catch (Exception ex)
            {
                ServerLog($"Failed to Send Data To All Clients : {ex}", 1);
            }
        }

        #endregion

        public void StopService()
        {
            if (tcpServer != null)
            {
                tcpServer.Dispose();
                tcpServer = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Dispose();
                tcpClient = null;
            }

            serverRunning = false;
            connecting = false;

            if (reConnectionTween.KillTween())
                ClientLog("Re-Connecting To Server Stopped");

            UUtility.NetLog("TCP Service Stop", log);
        }

        public bool SameIPAddress(IPEndPoint endPoint, string ipPort)
        {
            string ipPort0 = $"{endPoint.Address}:{endPoint.Port}";
            return ipPort0 == ipPort;
        }

        private void OnDataReceive(object sender, Message message)
        {
            IPEndPoint messageAddress = message.Client.GetEndPoint();
            string ipPort = $"{messageAddress.Address}:{messageAddress.Port}";

            OnDataReceived?.Invoke(ipPort, message.Data);
        }

        private void ServerLog(string message, int logType = 0)
        {
            UUtility.NetLog(logType, $"[TCP Server] {message}", log);
        }

        private void ClientLog(string message, int logType = 0)
        {
            UUtility.NetLog(logType, $"[TCP Client] {message}", log);
        }
    }
}