using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
#nullable disable

public static class NetManager
{
    /// <summary>
    /// �����Socket
    /// </summary>
    public static Socket listenfd;
    /// <summary>
    /// �ͻ����ֵ�
    /// </summary>
    public static Dictionary<Socket, ClientState> states = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// ���ӷ�����
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public static void Connect(string ip, int port)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
        listenfd.Bind(ipEndPoint);
        listenfd.Listen(0);

        Console.WriteLine("�����������ɹ�");
        while (true)
        {

        }
    }
}
