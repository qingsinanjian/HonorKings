using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    private static Socket socket;
    /// <summary>
    /// 字节数组
    /// </summary>
    private static ByteArray byteArray;
    /// <summary>
    /// 消息列表
    /// </summary>
    private static List<MsgBase> msgList;
    /// <summary>
    /// 是否正在连接
    /// </summary>
    private static bool isConnecting;
    /// <summary>
    /// 是否正在关闭
    /// </summary>
    private static bool isClosing;
    /// <summary>
    /// 发送队列
    /// </summary>
    private static Queue<ByteArray> writeQueue;

    private static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        msgList = new List<MsgBase>();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
    }

    public static void Connect(string ip, int port)
    {
        if(socket != null && socket.Connected)
        {
            Debug.Log("已经连接，请勿重复连接！");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("正在连接中，请稍后再试！");
            return;
        }
        //初始化
        Init();
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    /// <summary>
    /// 连接回调函数
    /// </summary>
    /// <param name="ar"></param>
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("连接成功");
            isConnecting = false;
            //接收消息
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("连接失败: " + e.Message);
            isConnecting = false;
        }
    }

    /// <summary>
    /// 接收消息回调函数
    /// </summary>
    /// <param name="ar"></param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //接受的数据量
            int count = socket.EndReceive(ar);
            //断开连接
            if (count == 0)
            {
                Close();
                return;
            }
            //接收数据
            byteArray.writeIndex += count;

            //处理消息
            OnReceiveData();
            //如果长度过小，扩容
            if (byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(byteArray.bytes.Length * 2);
            }
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("接收失败: " + e.Message);
        }
    }

    private static void Close()
    {
        if(socket != null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
            return;
        //消息还没有发送完
        if(writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
        }
    }

    /// <summary>
    /// 处理接收的数据
    /// </summary>
    private static void OnReceiveData()
    {
        if (byteArray.Length <= 2)
        {
            return;
        }
        byte[] bytes = byteArray.bytes;
        int readIndex = byteArray.readIndex;
        //short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        short length = (short)((bytes[readIndex] & 0x00FF) | (bytes[readIndex + 1] << 8));

        if (byteArray.Length < length + 2)
        {
            return;
        }
        byteArray.readIndex += 2;
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);

        if (string.IsNullOrEmpty(protoName))
        {
            Debug.LogError("协议名为空");
            return;
        }
        byteArray.readIndex += nameCount;

        //解析协议体
        int bodyLength = length - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
        byteArray.readIndex += bodyLength;

        //移动数据
        byteArray.MoveBytes();

        //添加到消息列表
        lock (msgList)
        {
            msgList.Add(msgBase);
        }

        //继续接收数据
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }

    /// <summary>
    /// 发送协议
    /// </summary>
    /// <param name="msgBase"></param>
    public static void Send(MsgBase msgBase)
    {
        if(socket == null || !socket.Connected)
        {
            Debug.LogError("Socket未连接");
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if(isClosing)
        {
            return;
        }
        //编码
        byte[] nameBytes = MsgBase.EncodeName(msgBase);
        byte[] bodyBytes = MsgBase.Encode(msgBase);
        //消息长度
        int length = nameBytes.Length + bodyBytes.Length;
        //消息体
        byte[] sendBytes = new byte[2 + length];
        sendBytes[0] = (byte)(length % 256);
        sendBytes[1] = (byte)(length / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        
        ByteArray byteArray = new ByteArray(sendBytes);
        int count = 0;
        lock(writeQueue)
        {
            writeQueue.Enqueue(byteArray);
            count = writeQueue.Count;
        }
        if(count == 1)
        {
            socket.BeginSend(byteArray.bytes, 0, byteArray.Length, SocketFlags.None, SendCallback, socket);
        }
    }

    /// <summary>
    /// 发送回调函数
    /// </summary>
    /// <param name="ar"></param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if(socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);

        ByteArray ba;
        lock(writeQueue)
        {
            ba = writeQueue.First();
        }

        ba.readIndex += count;
        //如果发送完毕，移除队列
        if(ba.Length == 0)
        {
            lock(writeQueue)
            {
                //清除
                writeQueue.Dequeue();
                //取到下一个
                ba = writeQueue.First();
            }
        }
        //如果还有数据，继续发送
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, SocketFlags.None, SendCallback, socket);
        }
        if (isClosing)
        {
            socket.Close();
        }
    }
}
