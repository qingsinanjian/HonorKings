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
    /// 服务端Socket
    /// </summary>
    public static Socket listenfd;
    /// <summary>
    /// 客户端字典
    /// </summary>
    public static Dictionary<Socket, ClientState> states = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// 用于检测的列表
    /// </summary>
    public static List<Socket> sockets = new List<Socket>();
    /// <summary>
    /// 连接服务器
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
        
        Console.WriteLine("服务器启动成功");
        while (true)
        {
            sockets.Clear();
            //放服务端的socket
            sockets.Add(listenfd);
            //放客户端的socket
            foreach (Socket socket in states.Keys)
            {
                sockets.Add(socket);
            }
            Socket.Select(sockets, null, null, 1000);
            for (int i = 0; i < sockets.Count; i++)
            {
                Socket s = sockets[i];
                if(s == listenfd)
                {
                    //有客户端连接
                    Accept(s);
                }
                else
                {
                    //客户端有消息发送过来
                }
            }
        }
    }

    /// <summary>
    /// 接收
    /// </summary>
    /// <param name="listenfd"></param>
    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            Console.WriteLine("有客户端连接成功" + socket.RemoteEndPoint.ToString());
            ClientState clientState = new ClientState();
            clientState.socket = socket;
            states.Add(socket, clientState);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept失败" + e.Message);
        }
    }
}
