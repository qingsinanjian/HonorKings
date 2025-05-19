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

    private static List<MsgBase> msgList;

    private static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        msgList = new List<MsgBase>();
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

    /// <summary>
    /// ������յ�����
    /// </summary>
    private static void OnReceiveData()
    {
        if(byteArray.Length <= 2)
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

        if(string.IsNullOrEmpty(protoName))
        {
            Debug.LogError("Э����Ϊ��");
            return;
        }
        byteArray.readIndex += nameCount;

        //����Э����
        int bodyLength = length - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
        byteArray.readIndex += bodyLength;

        //�ƶ�����
        byteArray.MoveBytes();

        //��ӵ���Ϣ�б�
        lock(msgList)
        {
            msgList.Add(msgBase);
        }

        //������������
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }
}
