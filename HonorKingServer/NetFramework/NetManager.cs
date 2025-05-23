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
//    /// 服务端Socket
//    /// </summary>
//    public static Socket listenfd;
//    /// <summary>
//    /// 客户端字典
//    /// </summary>
//    public static Dictionary<Socket, ClientState> states = new Dictionary<Socket, ClientState>();
//    /// <summary>
//    /// 用于检测的列表
//    /// </summary>
//    public static List<Socket> sockets = new List<Socket>();

//    private static float pingInterval = 2;
//    /// <summary>
//    /// 连接服务器
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

//        Console.WriteLine("服务器启动成功");
//        while (true)
//        {
//            sockets.Clear();
//            //放服务端的socket
//            sockets.Add(listenfd);
//            //放客户端的socket
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
//                    //有客户端连接
//                    Accept(s);
//                }
//                else
//                {
//                    //客户端有消息发送过来
//                    Receive(s);
//                }
//            }
//            CheckPing();
//        }
//    }

//    /// <summary>
//    /// 接收
//    /// </summary>
//    /// <param name="listenfd"></param>
//    private static void Accept(Socket listenfd)
//    {
//        try
//        {
//            Socket socket = listenfd.Accept();
//            Console.WriteLine("Accept成功" + socket.RemoteEndPoint.ToString());
//            ClientState state = new ClientState();
//            state.socket = socket;
//            state.lastPingTime = GetTimeStamp();
//            states.Add(socket, state);
//        }
//        catch (SocketException e)
//        {
//            Console.WriteLine("Accept失败" + e.Message);
//        }
//    }

//    /// <summary>
//    /// 接收客户端发过来的消息
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
//            Console.WriteLine("Receive失败，数组不够大");
//            return;
//        }

//        int count = 0;
//        try
//        {
//            count = socket.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.Remain, SocketFlags.None);
//        }
//        catch (SocketException e)
//        {
//            Console.WriteLine("Receive 失败," + e.Message);
//            return;
//        }

//        //客户端主动关闭
//        if(count <= 0)
//        {
//            Console.WriteLine("Socket Close:" + socket.RemoteEndPoint.ToString());
//            return;
//        }
//        readBuffer.writeIndex += count;
//        //处理消息
//        OnReceiveData(clientState);
//        readBuffer.MoveBytes();
//    }

//    /// <summary>
//    /// 处理消息
//    /// </summary>
//    /// <param name="state">客户端对象</param>
//    private static void OnReceiveData(ClientState state)
//    {
//        ByteArray readBuffer = state.readBuffer;
//        byte[] bytes = readBuffer.bytes;
//        int readIndex = readBuffer.readIndex;

//        if (readBuffer.Length < 2)
//        {
//            return;
//        }
//        //解析总长度
//        short length = (short)(bytes[readIndex] + (bytes[readIndex + 1] << 8));
//        //收到的消息没有解析出来的多
//        if(readBuffer.Length < length)
//        {
//            return;
//        }
//        readBuffer.readIndex += 2;

//        int nameCount = 0;
//        string protoName = MsgBase.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);
//        if(protoName == "")
//        {
//            Console.WriteLine("OnReceiveData 失败，协议名为空");
//            return;
//        }
//        readBuffer.readIndex += nameCount;

//        //解析消息体
//        int bodyLength = length - nameCount;
//        MsgBase msgBase = MsgBase.Decode(protoName, readBuffer.bytes, readBuffer.readIndex, bodyLength);
//        readBuffer.readIndex += bodyLength;
//        readBuffer.MoveBytes();

//        //通过反射调用客户端发过来的协议对应的方法
//        MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protoName);
//        Console.WriteLine("ReceiveData:" + protoName + " " + state.socket.RemoteEndPoint.ToString());
//        if(methodInfo != null)
//        {
//            //要执行方法的参数
//            object[] o = { state, msgBase };
//            //调用方法
//            methodInfo.Invoke(null, o);
//        }
//        else
//        {
//            Console.WriteLine("OnReceiveData 失败，方法不存在" + protoName);
//        }

//        if(readBuffer.Length > 2)
//        {
//            OnReceiveData(state);
//        }
//    }

//    /// <summary>
//    /// 发送消息
//    /// </summary>
//    /// <param name="state">客户端对象</param>
//    /// <param name="msgBase">消息</param>
//    public static void Send(ClientState state, MsgBase msgBase)
//    {
//        if(state == null || state.socket == null || !state.socket.Connected)
//        {
//            Console.WriteLine("Send 失败，客户端对象为空或socket为空或未连接");
//            return;
//        }

//        //编码
//        byte[] nameBytes = MsgBase.EncodeName(msgBase);
//        byte[] bodyBytes = MsgBase.Encode(msgBase);
//        //消息长度
//        int length = nameBytes.Length + bodyBytes.Length;
//        //消息体
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
//            Console.WriteLine("Send 失败" + e.Message);
//        }
//    }

//    /// <summary>
//    /// 关闭对应的客户端
//    /// </summary>
//    /// <param name="state"></param>
//    private static void Close(ClientState state)
//    {
//        state.socket.Close();
//        states.Remove(state.socket);
//    }

//    /// <summary>
//    /// 获取时间戳
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
//                Console.WriteLine("心跳机制，断开连接" + state.socket.RemoteEndPoint.ToString());
//                //关闭客户端
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

    private static float pingInterval = 2;
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
                if (s == listenfd)
                {
                    //有客户端连接
                    Accept(s);
                }
                else
                {
                    //客户端有消息发送过来
                    Receive(s);
                }
            }
            CheckPing();
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
            Console.WriteLine("Accept成功" + socket.RemoteEndPoint.ToString());
            ClientState state = new ClientState();
            state.socket = socket;
            state.lastPingTime = GetTimeStamp();
            states.Add(socket, state);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept失败" + e.Message);
        }
    }

    /// <summary>
    /// 接收客户端发过来的消息
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
            Console.WriteLine("Receive失败，数组不够大");
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
            Console.WriteLine("Receive 失败," + e.Message);
            Close(clientState);
            return;
        }

        //客户端主动关闭
        if (count <= 0)
        {
            Console.WriteLine("Socket Close:" + socket.RemoteEndPoint.ToString());
            Close(clientState);
            return;
        }
        readBuffer.writeIndex += count;
        //处理消息
        OnReceiveData(clientState);
        readBuffer.MoveBytes();
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="state">客户端对象</param>
    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.readBuffer;
        byte[] bytes = readBuffer.bytes;
        int readIndex = readBuffer.readIndex;

        if (readBuffer.Length < 2)
        {
            return;
        }
        //解析总长度
        short length = (short)(bytes[readIndex] + (bytes[readIndex + 1] << 8));
        //收到的消息没有解析出来的多
        if (readBuffer.Length < length)
        {
            return;
        }
        readBuffer.readIndex += 2;

        int nameCount = 0;
        string protoName = ProtobufTool.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData 失败，协议名为空");
            Close(state);
            return;
        }
        readBuffer.readIndex += nameCount;

        //解析消息体
        int bodyLength = length - nameCount;
        IExtensible msgBase = ProtobufTool.Decode(protoName, readBuffer.bytes, readBuffer.readIndex, bodyLength);
        readBuffer.readIndex += bodyLength;
        readBuffer.MoveBytes();

        //通过反射调用客户端发过来的协议对应的方法
        MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protoName);
        Console.WriteLine("ReceiveData:" + protoName + " " + state.socket.RemoteEndPoint.ToString());
        if (methodInfo != null)
        {
            //要执行方法的参数
            object[] o = { state, msgBase };
            //调用方法
            methodInfo.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData 失败，方法不存在" + protoName);
        }

        if (readBuffer.Length > 2)
        {
            OnReceiveData(state);
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="state">客户端对象</param>
    /// <param name="msgBase">消息</param>
    public static void Send(ClientState state, IExtensible msgBase)
    {
        if (state == null || state.socket == null || !state.socket.Connected)
        {
            Console.WriteLine("Send 失败，客户端对象为空或socket为空或未连接");
            return;
        }

        //编码
        byte[] nameBytes = ProtobufTool.EncodeName(msgBase);
        byte[] bodyBytes = ProtobufTool.Encode(msgBase);
        //消息长度
        int length = nameBytes.Length + bodyBytes.Length;
        //消息体
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
            Console.WriteLine("Send 失败" + e.Message);
        }
    }

    /// <summary>
    /// 关闭对应的客户端
    /// </summary>
    /// <param name="state"></param>
    private static void Close(ClientState state)
    {
        state.socket.Close();
        states.Remove(state.socket);
    }

    /// <summary>
    /// 获取时间戳
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
                Console.WriteLine("心跳机制，断开连接" + state.socket.RemoteEndPoint.ToString());
                //关闭客户端
                Close(state);
                return;
            }
        }
    }
}

