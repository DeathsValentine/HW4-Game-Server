using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Server
{
    public static int maxPlayers { get; private set; }
    public static int port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener TCPListener;
    private static UdpClient UDPListener;

    public static void Start(int _maxPlayers, int _port)
    {
        maxPlayers = _maxPlayers;
        port = _port;

        Debug.Log("Starting server...");

        InitializeServerData();

        TCPListener = new TcpListener(IPAddress.Any, port);
        TCPListener.Start();
        TCPListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null); // Originally TCPConnectionCallback

        UDPListener = new UdpClient(port);
        UDPListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log("Server started on {port}");
    }

    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = TCPListener.EndAcceptTcpClient(_result);
        TCPListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null); // Originally TCPConnectionCallback

        Debug.Log("Incoming connetion from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= maxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log("{_client.Client.RemoteEndPoint} failed to connect: Server full");
    }

    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = UDPListener.EndReceive(_result, ref _clientEndPoint);
            UDPListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientID = _packet.ReadInt();
                if (_clientID == 0)
                {
                    return;
                }

                if (clients[_clientID].udp.endPoint == null)
                {
                    clients[_clientID].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientID].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientID].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log("Error receiving UDP data: {_ex}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                UDPListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log("Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>(){
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.udpTestReceived, ServerHandle.UDPTestReceived }
            };

        Debug.Log("Initialized packets.");
    }
}
