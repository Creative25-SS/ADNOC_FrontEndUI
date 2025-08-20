
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;
using SimpleUdp;
using System.Text;

namespace TJ.Networking
{
    public class UDPServer : MonoBehaviour
    {
        private UdpEndpoint udp;
        
        private string localIpAddress; // Will be set at runtime
        [SerializeField] private int localPort = 8000;
        
        [SerializeField] private string clientIpAddress = "127.0.0.1";
        [SerializeField] private int clientPort = 8001;
        
        public static event Action<string> OnUDPServerStartFailed;
        public static event Action OnUDPServerStarted;
        
        public delegate void DatagramReceivedEventHandler(string senderIp, int senderPort, string message);
        public static event DatagramReceivedEventHandler OnDatagramReceived;

        private void Awake()
        {
            localIpAddress = GetLocalIPAddress();
        }

        private void Start()
        {
            StartUDPServer();
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                
                var ipAddress = host.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork 
                                        && !IPAddress.IsLoopback(ip));
                
                if (ipAddress != null)
                {
                    return ipAddress.ToString();
                }
                
                Debug.LogWarning("No suitable IPv4 address found, falling back to localhost");
                return "127.0.0.1";
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting local IP address: {e.Message}");
                return "127.0.0.1";
            }
        }

        private void StartUDPServer()
        {
            try
            {
                udp = new UdpEndpoint(localIpAddress, localPort);
                
                udp.EndpointDetected += EndpointDetected;
                udp.DatagramReceived += DatagramReceived;
            
                Debug.Log($"UDP server started on {localIpAddress}:{localPort}");
                OnUDPServerStarted?.Invoke();

                SendMsg("server");
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Server failed to start {localIpAddress}:{localPort}");
                Debug.LogError(e.Message);
                OnUDPServerStartFailed?.Invoke(e.Message);
            }
        }
        
        public void RetryConnection()
        {
            StartUDPServer();
        }
        
        private void OnDestroy()
        {
            udp?.Dispose();
            Debug.Log("UDP server closed.");
        }
        
        public void SendMsg(string message)
        {
            udp?.Send(clientIpAddress, clientPort, message);
            Debug.Log($"<color=yellow>Message sent to {clientIpAddress}:{clientPort} | {message}</color>");
        }
        
        private static void EndpointDetected(object sender, EndpointMetadata metadata)
        {
            Debug.Log($"Endpoint detected: {metadata.Ip}:{metadata.Port}");
        }
        
        private static void DatagramReceived(object sender, Datagram dg)
        {
            string receivedMessage = Encoding.UTF8.GetString(dg.Data);
            Debug.Log($"<color=yellow>[{dg.Ip}:{dg.Port}]: {receivedMessage}</color>");
            MainThreadDispatcher.ExecuteInUpdate(() => { OnDatagramReceived?.Invoke(dg.Ip, dg.Port, receivedMessage); });
        }

        public void SetClientInfo(string ip, int port)
        {
            clientIpAddress = ip;
            clientPort = port;
        }

        public (string ip, int port) GetClientInfo()
        {
            return (clientIpAddress, clientPort);
        }

        public string GetLocalIP()
        {
            return localIpAddress;
        }
    }
}