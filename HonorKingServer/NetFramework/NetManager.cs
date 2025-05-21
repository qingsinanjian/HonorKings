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
    /// ���ڼ����б�
    /// </summary>
    public static List<Socket> sockets = new List<Socket>();
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
            sockets.Clear();
            //�ŷ���˵�socket
            sockets.Add(listenfd);
            //�ſͻ��˵�socket
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
                    //�пͻ�������
                    Accept(s);
                }
                else
                {
                    //�ͻ�������Ϣ���͹���
                    Receive(s);
                }
            }
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="listenfd"></param>
    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            Console.WriteLine("Accept�ɹ�" + socket.RemoteEndPoint.ToString());
            ClientState clientState = new ClientState();
            clientState.socket = socket;
            states.Add(socket, clientState);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Acceptʧ��" + e.Message);
        }
    }

    /// <summary>
    /// ���տͻ��˷���������Ϣ
    /// </summary>
    /// <param name="s"></param>
    private static void Receive(Socket socket)
    {
        ClientState clientState = states[socket];
        ByteArray readBuffer = clientState.readBuffer;

        if(readBuffer.Remain <= 0)
        {
            readBuffer.MoveBytes();
        }
        if(readBuffer.Remain <= 0)
        {
            Console.WriteLine("Receiveʧ�ܣ����鲻����");
            return;
        }

        int count = 0;
        try
        {
            count = socket.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.Remain, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive ʧ��," + e.Message);
            return;
        }

        //�ͻ��������ر�
        if(count <= 0)
        {
            Console.WriteLine("Socket Close:" + socket.RemoteEndPoint.ToString());
            return;
        }
        readBuffer.writeIndex += count;
        //������Ϣ
        OnReceiveData(clientState);
        readBuffer.MoveBytes();
    }

    private static void OnReceiveData(ClientState state)
    {
        
    }
}
