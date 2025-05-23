//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Reflection;
//#nullable disable

//public static class NetManager
//{
//    /// <summary>
//    /// �����Socket
//    /// </summary>
//    public static Socket listenfd;
//    /// <summary>
//    /// �ͻ����ֵ�
//    /// </summary>
//    public static Dictionary<Socket, ClientState> states = new Dictionary<Socket, ClientState>();
//    /// <summary>
//    /// ���ڼ����б�
//    /// </summary>
//    public static List<Socket> sockets = new List<Socket>();

//    private static float pingInterval = 2;
//    /// <summary>
//    /// ���ӷ�����
//    /// </summary>
//    /// <param name="ip"></param>
//    /// <param name="port"></param>
//    public static void Connect(string ip, int port)
//    {
//        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//        IPAddress ipAddress = IPAddress.Parse(ip);
//        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
//        listenfd.Bind(ipEndPoint);
//        listenfd.Listen(0);

//        Console.WriteLine("�����������ɹ�");
//        while (true)
//        {
//            sockets.Clear();
//            //�ŷ���˵�socket
//            sockets.Add(listenfd);
//            //�ſͻ��˵�socket
//            foreach (Socket socket in states.Keys)
//            {
//                sockets.Add(socket);
//            }
//            Socket.Select(sockets, null, null, 1000);
//            for (int i = 0; i < sockets.Count; i++)
//            {
//                Socket s = sockets[i];
//                if(s == listenfd)
//                {
//                    //�пͻ�������
//                    Accept(s);
//                }
//                else
//                {
//                    //�ͻ�������Ϣ���͹���
//                    Receive(s);
//                }
//            }
//            CheckPing();
//        }
//    }

//    /// <summary>
//    /// ����
//    /// </summary>
//    /// <param name="listenfd"></param>
//    private static void Accept(Socket listenfd)
//    {
//        try
//        {
//            Socket socket = listenfd.Accept();
//            Console.WriteLine("Accept�ɹ�" + socket.RemoteEndPoint.ToString());
//            ClientState state = new ClientState();
//            state.socket = socket;
//            state.lastPingTime = GetTimeStamp();
//            states.Add(socket, state);
//        }
//        catch (SocketException e)
//        {
//            Console.WriteLine("Acceptʧ��" + e.Message);
//        }
//    }

//    /// <summary>
//    /// ���տͻ��˷���������Ϣ
//    /// </summary>
//    /// <param name="s"></param>
//    private static void Receive(Socket socket)
//    {
//        ClientState clientState = states[socket];
//        ByteArray readBuffer = clientState.readBuffer;

//        if(readBuffer.Remain <= 0)
//        {
//            readBuffer.MoveBytes();
//        }
//        if(readBuffer.Remain <= 0)
//        {
//            Console.WriteLine("Receiveʧ�ܣ����鲻����");
//            return;
//        }

//        int count = 0;
//        try
//        {
//            count = socket.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.Remain, SocketFlags.None);
//        }
//        catch (SocketException e)
//        {
//            Console.WriteLine("Receive ʧ��," + e.Message);
//            return;
//        }

//        //�ͻ��������ر�
//        if(count <= 0)
//        {
//            Console.WriteLine("Socket Close:" + socket.RemoteEndPoint.ToString());
//            return;
//        }
//        readBuffer.writeIndex += count;
//        //������Ϣ
//        OnReceiveData(clientState);
//        readBuffer.MoveBytes();
//    }

//    /// <summary>
//    /// ������Ϣ
//    /// </summary>
//    /// <param name="state">�ͻ��˶���</param>
//    private static void OnReceiveData(ClientState state)
//    {
//        ByteArray readBuffer = state.readBuffer;
//        byte[] bytes = readBuffer.bytes;
//        int readIndex = readBuffer.readIndex;

//        if (readBuffer.Length < 2)
//        {
//            return;
//        }
//        //�����ܳ���
//        short length = (short)(bytes[readIndex] + (bytes[readIndex + 1] << 8));
//        //�յ�����Ϣû�н��������Ķ�
//        if(readBuffer.Length < length)
//        {
//            return;
//        }
//        readBuffer.readIndex += 2;

//        int nameCount = 0;
//        string protoName = MsgBase.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);
//        if(protoName == "")
//        {
//            Console.WriteLine("OnReceiveData ʧ�ܣ�Э����Ϊ��");
//            return;
//        }
//        readBuffer.readIndex += nameCount;

//        //������Ϣ��
//        int bodyLength = length - nameCount;
//        MsgBase msgBase = MsgBase.Decode(protoName, readBuffer.bytes, readBuffer.readIndex, bodyLength);
//        readBuffer.readIndex += bodyLength;
//        readBuffer.MoveBytes();

//        //ͨ��������ÿͻ��˷�������Э���Ӧ�ķ���
//        MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protoName);
//        Console.WriteLine("ReceiveData:" + protoName + " " + state.socket.RemoteEndPoint.ToString());
//        if(methodInfo != null)
//        {
//            //Ҫִ�з����Ĳ���
//            object[] o = { state, msgBase };
//            //���÷���
//            methodInfo.Invoke(null, o);
//        }
//        else
//        {
//            Console.WriteLine("OnReceiveData ʧ�ܣ�����������" + protoName);
//        }

//        if(readBuffer.Length > 2)
//        {
//            OnReceiveData(state);
//        }
//    }

//    /// <summary>
//    /// ������Ϣ
//    /// </summary>
//    /// <param name="state">�ͻ��˶���</param>
//    /// <param name="msgBase">��Ϣ</param>
//    public static void Send(ClientState state, MsgBase msgBase)
//    {
//        if(state == null || state.socket == null || !state.socket.Connected)
//        {
//            Console.WriteLine("Send ʧ�ܣ��ͻ��˶���Ϊ�ջ�socketΪ�ջ�δ����");
//            return;
//        }

//        //����
//        byte[] nameBytes = MsgBase.EncodeName(msgBase);
//        byte[] bodyBytes = MsgBase.Encode(msgBase);
//        //��Ϣ����
//        int length = nameBytes.Length + bodyBytes.Length;
//        //��Ϣ��
//        byte[] sendBytes = new byte[2 + length];
//        sendBytes[0] = (byte)(length % 256);
//        sendBytes[1] = (byte)(length / 256);
//        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
//        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

//        try
//        {
//            state.socket.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
//        }
//        catch (SocketException e)
//        {
//            Console.WriteLine("Send ʧ��" + e.Message);
//        }
//    }

//    /// <summary>
//    /// �رն�Ӧ�Ŀͻ���
//    /// </summary>
//    /// <param name="state"></param>
//    private static void Close(ClientState state)
//    {
//        state.socket.Close();
//        states.Remove(state.socket);
//    }

//    /// <summary>
//    /// ��ȡʱ���
//    /// </summary>
//    /// <returns></returns>
//    public static long GetTimeStamp()
//    {
//        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//        return Convert.ToInt64(ts.TotalSeconds);
//    }

//    private static void CheckPing()
//    {
//        foreach (var state in states.Values)
//        {
//            if (GetTimeStamp() - state.lastPingTime > pingInterval * 4)
//            {
//                Console.WriteLine("�������ƣ��Ͽ�����" + state.socket.RemoteEndPoint.ToString());
//                //�رտͻ���
//                Close(state);
//                return;
//            }
//        }
//    }
//}

using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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

    private static float pingInterval = 2;
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
                if (s == listenfd)
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
            CheckPing();
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
            ClientState state = new ClientState();
            state.socket = socket;
            state.lastPingTime = GetTimeStamp();
            states.Add(socket, state);
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

        if (readBuffer.Remain <= 0)
        {
            readBuffer.MoveBytes();
        }
        if (readBuffer.Remain <= 0)
        {
            Console.WriteLine("Receiveʧ�ܣ����鲻����");
            Close(clientState);
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
            Close(clientState);
            return;
        }

        //�ͻ��������ر�
        if (count <= 0)
        {
            Console.WriteLine("Socket Close:" + socket.RemoteEndPoint.ToString());
            Close(clientState);
            return;
        }
        readBuffer.writeIndex += count;
        //������Ϣ
        OnReceiveData(clientState);
        readBuffer.MoveBytes();
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="state">�ͻ��˶���</param>
    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.readBuffer;
        byte[] bytes = readBuffer.bytes;
        int readIndex = readBuffer.readIndex;

        if (readBuffer.Length < 2)
        {
            return;
        }
        //�����ܳ���
        short length = (short)(bytes[readIndex] + (bytes[readIndex + 1] << 8));
        //�յ�����Ϣû�н��������Ķ�
        if (readBuffer.Length < length)
        {
            return;
        }
        readBuffer.readIndex += 2;

        int nameCount = 0;
        string protoName = ProtobufTool.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData ʧ�ܣ�Э����Ϊ��");
            Close(state);
            return;
        }
        readBuffer.readIndex += nameCount;

        //������Ϣ��
        int bodyLength = length - nameCount;
        IExtensible msgBase = ProtobufTool.Decode(protoName, readBuffer.bytes, readBuffer.readIndex, bodyLength);
        readBuffer.readIndex += bodyLength;
        readBuffer.MoveBytes();

        //ͨ��������ÿͻ��˷�������Э���Ӧ�ķ���
        MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protoName);
        Console.WriteLine("ReceiveData:" + protoName + " " + state.socket.RemoteEndPoint.ToString());
        if (methodInfo != null)
        {
            //Ҫִ�з����Ĳ���
            object[] o = { state, msgBase };
            //���÷���
            methodInfo.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData ʧ�ܣ�����������" + protoName);
        }

        if (readBuffer.Length > 2)
        {
            OnReceiveData(state);
        }
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="state">�ͻ��˶���</param>
    /// <param name="msgBase">��Ϣ</param>
    public static void Send(ClientState state, IExtensible msgBase)
    {
        if (state == null || state.socket == null || !state.socket.Connected)
        {
            Console.WriteLine("Send ʧ�ܣ��ͻ��˶���Ϊ�ջ�socketΪ�ջ�δ����");
            return;
        }

        //����
        byte[] nameBytes = ProtobufTool.EncodeName(msgBase);
        byte[] bodyBytes = ProtobufTool.Encode(msgBase);
        //��Ϣ����
        int length = nameBytes.Length + bodyBytes.Length;
        //��Ϣ��
        byte[] sendBytes = new byte[2 + length];
        sendBytes[0] = (byte)(length % 256);
        sendBytes[1] = (byte)(length / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        try
        {
            state.socket.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Send ʧ��" + e.Message);
        }
    }

    /// <summary>
    /// �رն�Ӧ�Ŀͻ���
    /// </summary>
    /// <param name="state"></param>
    private static void Close(ClientState state)
    {
        state.socket.Close();
        states.Remove(state.socket);
    }

    /// <summary>
    /// ��ȡʱ���
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    private static void CheckPing()
    {
        foreach (var state in states.Values)
        {
            if (GetTimeStamp() - state.lastPingTime > pingInterval * 4)
            {
                Console.WriteLine("�������ƣ��Ͽ�����" + state.socket.RemoteEndPoint.ToString());
                //�رտͻ���
                Close(state);
                return;
            }
        }
    }
}

