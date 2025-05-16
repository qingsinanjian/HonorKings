using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    private static Socket socket;
    /// <summary>
    /// �ֽ�����
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
    /// ���ӻص�����
    /// </summary>
    /// <param name="ar"></param>
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("���ӳɹ�");
            //������Ϣ
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("����ʧ��: " + e.Message);
        }
    }

    /// <summary>
    /// ������Ϣ�ص�����
    /// </summary>
    /// <param name="ar"></param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //���ܵ�������
            int count = socket.EndReceive(ar);
            //�Ͽ�����
            if(count == 0)
            {
                Close();
                return;
            }
            //��������
            byteArray.writeIndex += count;

            //������Ϣ
            OnReceiveData();
            //������ȹ�С������
            if(byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(byteArray.bytes.Length * 2);
            }
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("����ʧ��: " + e.Message);
        }
    }

    private static void Close()
    {
        
    }

    private static void OnReceiveData()
    {

    }
}
