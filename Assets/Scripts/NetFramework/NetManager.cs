using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    private static Socket socket;
    /// <summary>
    /// 字节数组
    /// </summary>
    private static ByteArray byteArray;

    private static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
    }

    public static void Connect(string ip, int port)
    {
        if (socket == null)
        {
            Init();
        }

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
            //接收消息
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("连接失败: " + e.Message);
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
            if(count == 0)
            {
                Close();
                return;
            }
            //接收数据
            byteArray.writeIndex += count;

            //处理消息
            OnReceiveData();
            //如果长度过小，扩容
            if(byteArray.Remain < 8)
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
        
    }

    private static void OnReceiveData()
    {

    }
}
