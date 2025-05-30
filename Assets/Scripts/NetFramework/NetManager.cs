//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using UnityEngine;

//public static class NetManager
//{
//    private static Socket socket;
//    /// <summary>
//    /// �ֽ�����
//    /// </summary>
//    private static ByteArray byteArray;
//    /// <summary>
//    /// ��Ϣ�б�
//    /// </summary>
//    private static List<MsgBase> msgList;
//    /// <summary>
//    /// �Ƿ���������
//    /// </summary>
//    private static bool isConnecting;
//    /// <summary>
//    /// �Ƿ����ڹر�
//    /// </summary>
//    private static bool isClosing;
//    /// <summary>
//    /// ���Ͷ���
//    /// </summary>
//    private static Queue<ByteArray> writeQueue;
//    /// <summary>
//    /// һ֡����������Ϣ����
//    /// </summary>
//    private static int processMsgCount = 10;
//    /// <summary>
//    /// �Ƿ�������������
//    /// </summary>
//    private static bool isUsePing = true;
//    /// <summary>
//    /// ��һ�η���ping��ʱ��
//    /// </summary>
//    private static float lastPingTime = 0;
//    /// <summary>
//    /// ��һ�ν���pong��ʱ��
//    /// </summary>
//    private static float lastPongTime = 0;
//    /// <summary>
//    /// �������Ƶļ��ʱ��
//    /// </summary>
//    private static float pingInterval = 2f;
//    /// <summary>
//    /// �����¼�
//    /// </summary>
//    public enum NetEvent
//    {
//        ConnectSucc = 1,
//        ConnectFail = 2,
//        Close = 3,
//    }
//    /// <summary>
//    /// Ҫִ�е��¼�
//    /// </summary>
//    /// <param name="err"></param>
//    public delegate void EventListener(string err);
//    /// <summary>
//    /// �¼����ֵ�
//    /// </summary>
//    private static Dictionary<NetEvent, EventListener> eventListener = new Dictionary<NetEvent, EventListener>();
//    /// <summary>
//    /// ����¼�����
//    /// </summary>
//    /// <param name="netEvent"></param>
//    /// <param name="listener"></param>
//    public static void AddEventListener(NetEvent netEvent, EventListener listener)
//    {
//        if (eventListener.ContainsKey(netEvent))
//        {
//            eventListener[netEvent] += listener;
//        }
//        else
//        {
//            eventListener.Add(netEvent, listener);
//        }
//    }
//    /// <summary>
//    /// �Ƴ��¼�����
//    /// </summary>
//    /// <param name="netEvent"></param>
//    /// <param name="listener"></param>
//    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
//    {
//        if (eventListener.ContainsKey(netEvent))
//        {
//            eventListener[netEvent] -= listener;
//            if (eventListener[netEvent] == null)
//            {
//                eventListener.Remove(netEvent);
//            }
//        }
//    }
//    /// <summary>
//    /// �ַ��¼�
//    /// </summary>
//    /// <param name="netEvent"></param>
//    /// <param name="err"></param>
//    public static void FireEvent(NetEvent netEvent, string err = "")
//    {
//        if (eventListener.ContainsKey(netEvent))
//        {
//            eventListener[netEvent].Invoke(err);
//        }
//    }
//    /// <summary>
//    /// ��Ϣ����ί��
//    /// </summary>
//    /// <param name="msgBase"></param>
//    public delegate void MsgListener(MsgBase msgBase);
//    /// <summary>
//    /// ��Ϣ�¼��ֵ�
//    /// </summary>
//    private static Dictionary<string, MsgListener> msgListener = new Dictionary<string, MsgListener>();
//    /// <summary>
//    /// ����¼�����
//    /// </summary>
//    /// <param name="msgName"></param>
//    /// <param name="listener"></param>
//    public static void AddMsgListener(string msgName, MsgListener listener)
//    {
//        if (msgListener.ContainsKey(msgName))
//        {
//            msgListener[msgName] += listener;
//        }
//        else
//        {
//            msgListener.Add(msgName, listener);
//        }
//    }
//    /// <summary>
//    /// �Ƴ��¼�����
//    /// </summary>
//    /// <param name="msgName"></param>
//    /// <param name="listener"></param>
//    public static void RemoveMsgListener(string msgName, MsgListener listener)
//    {
//        if (msgListener.ContainsKey(msgName))
//        {
//            msgListener[msgName] -= listener;
//            if (msgListener[msgName] == null)
//            {
//                msgListener.Remove(msgName);
//            }
//        }
//    }
//    /// <summary>
//    /// �ⷢ��Ϣ�¼�
//    /// </summary>
//    /// <param name="msgName"></param>
//    /// <param name="msgBase"></param>
//    public static void FireMsg(string msgName, MsgBase msgBase)
//    {
//        if (msgListener.ContainsKey(msgName))
//        {
//            msgListener[msgName].Invoke(msgBase);
//        }
//    }

//    /// <summary>
//    /// ��ʼ��
//    /// </summary>
//    private static void Init()
//    {
//        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//        byteArray = new ByteArray();
//        msgList = new List<MsgBase>();
//        writeQueue = new Queue<ByteArray>();
//        isConnecting = false;
//        isClosing = false;

//        lastPingTime = Time.time;
//        lastPongTime = Time.time;

//        if(!msgListener.ContainsKey("MsgPong"))
//        {
//            AddMsgListener("MsgPong", OnMsgPong);
//        }
//    }

//    public static void Connect(string ip, int port)
//    {
//        if (socket != null && socket.Connected)
//        {
//            Debug.Log("�Ѿ����ӣ������ظ����ӣ�");
//            return;
//        }

//        if (isConnecting)
//        {
//            Debug.Log("���������У����Ժ����ԣ�");
//            return;
//        }
//        //��ʼ��
//        Init();
//        isConnecting = true;
//        socket.BeginConnect(ip, port, ConnectCallback, socket);
//    }

//    /// <summary>
//    /// ���ӻص�����
//    /// </summary>
//    /// <param name="ar"></param>
//    private static void ConnectCallback(IAsyncResult ar)
//    {
//        try
//        {
//            Socket socket = (Socket)ar.AsyncState;
//            socket.EndConnect(ar);
//            Debug.Log("���ӳɹ�");
//            FireEvent(NetEvent.ConnectSucc);
//            isConnecting = false;
//            //������Ϣ
//            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
//        }
//        catch (SocketException e)
//        {
//            Debug.LogError("����ʧ��: " + e.Message);
//            FireEvent(NetEvent.ConnectFail, e.Message);
//            isConnecting = false;
//        }
//    }

//    /// <summary>
//    /// ������Ϣ�ص�����
//    /// </summary>
//    /// <param name="ar"></param>
//    private static void ReceiveCallback(IAsyncResult ar)
//    {
//        try
//        {
//            Socket socket = (Socket)ar.AsyncState;
//            //���ܵ�������
//            int count = socket.EndReceive(ar);
//            //�Ͽ�����
//            if (count == 0)
//            {
//                Close();
//                return;
//            }
//            //��������
//            byteArray.writeIndex += count;

//            //������Ϣ
//            OnReceiveData();
//            //������ȹ�С������
//            if (byteArray.Remain < 8)
//            {
//                byteArray.MoveBytes();
//                byteArray.ReSize(byteArray.bytes.Length * 2);
//            }
//            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
//        }
//        catch (SocketException e)
//        {
//            Debug.LogError("����ʧ��: " + e.Message);
//        }
//    }

//    private static void Close()
//    {
//        if (socket == null || !socket.Connected)
//        {
//            return;
//        }
//        if (isConnecting)
//            return;
//        //��Ϣ��û�з�����
//        if (writeQueue.Count > 0)
//        {
//            isClosing = true;
//        }
//        else
//        {
//            socket.Close();
//            FireEvent(NetEvent.Close);
//        }
//    }

//    /// <summary>
//    /// ������յ�����
//    /// </summary>
//    private static void OnReceiveData()
//    {
//        if (byteArray.Length <= 2)
//        {
//            return;
//        }
//        byte[] bytes = byteArray.bytes;
//        int readIndex = byteArray.readIndex;
//        //short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
//        short length = (short)((bytes[readIndex] & 0x00FF) | (bytes[readIndex + 1] << 8));

//        if (byteArray.Length < length + 2)
//        {
//            return;
//        }
//        byteArray.readIndex += 2;
//        int nameCount = 0;
//        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);

//        if (string.IsNullOrEmpty(protoName))
//        {
//            Debug.LogError("Э����Ϊ��");
//            return;
//        }
//        byteArray.readIndex += nameCount;

//        //����Э����
//        int bodyLength = length - nameCount;
//        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
//        byteArray.readIndex += bodyLength;

//        //�ƶ�����
//        byteArray.MoveBytes();

//        //��ӵ���Ϣ�б�
//        lock (msgList)
//        {
//            msgList.Add(msgBase);
//        }

//        //������������
//        if (byteArray.Length > 2)
//        {
//            OnReceiveData();
//        }
//    }

//    /// <summary>
//    /// ����Э��
//    /// </summary>
//    /// <param name="msgBase"></param>
//    public static void Send(MsgBase msgBase)
//    {
//        if (socket == null || !socket.Connected)
//        {
//            Debug.LogError("Socketδ����");
//            return;
//        }
//        if (isConnecting)
//        {
//            return;
//        }
//        if (isClosing)
//        {
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

//        ByteArray byteArray = new ByteArray(sendBytes);
//        int count = 0;
//        lock (writeQueue)
//        {
//            writeQueue.Enqueue(byteArray);
//            count = writeQueue.Count;
//        }
//        if (count == 1)
//        {
//            socket.BeginSend(byteArray.bytes, 0, byteArray.Length, SocketFlags.None, SendCallback, socket);
//        }
//    }

//    /// <summary>
//    /// ���ͻص�����
//    /// </summary>
//    /// <param name="ar"></param>
//    private static void SendCallback(IAsyncResult ar)
//    {
//        Socket socket = ar.AsyncState as Socket;
//        if (socket == null || !socket.Connected)
//        {
//            return;
//        }
//        int count = socket.EndSend(ar);

//        ByteArray ba;
//        lock (writeQueue)
//        {
//            ba = writeQueue.First();
//        }

//        ba.readIndex += count;
//        //���������ϣ��Ƴ�����
//        if (ba.Length == 0)
//        {
//            lock (writeQueue)
//            {
//                //���
//                writeQueue.Dequeue();
//                //ȡ����һ��
//                ba = writeQueue.First();
//            }
//        }
//        //����������ݣ���������
//        if (ba != null)
//        {
//            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, SocketFlags.None, SendCallback, socket);
//        }
//        if (isClosing)
//        {
//            socket.Close();
//        }
//    }

//    /// <summary>
//    /// ������Ϣ
//    /// </summary>
//    private static void MsgUpdate()
//    {
//        //û����Ϣ
//        if(msgList.Count == 0)
//        {
//            return;
//        }

//        for (int i = 0; i < processMsgCount; i++)
//        {
//            MsgBase msgBase = null;
//            lock(msgList)
//            {
//                if(msgList.Count > 0)
//                {
//                    msgBase = msgList[0];
//                    msgList.RemoveAt(0);
//                }
//            }

//            if(msgBase != null)
//            {
//                FireMsg(msgBase.protoName, msgBase);
//            }
//            else
//            {
//                break;
//            }
//        }
//    }

//    private static void PingUpdate()
//    {
//        if(!isUsePing)
//        {
//            return;
//        }
//        if(Time.time - lastPingTime > pingInterval)
//        {
//            //����
//            MsgBase msgBase = new MsgPing();
//            Send(msgBase);
//            lastPingTime = Time.time;
//        }
//        //�Ͽ��Ĵ���
//        if(Time.time - lastPongTime > pingInterval * 4)
//        {
//            Debug.Log("Ping��ʱ��");
//            Close();
//        }
//    }

//    public static void Update()
//    {
//        PingUpdate();
//        MsgUpdate();
//    }

//    private static void OnMsgPong(MsgBase msgBase)
//    {
//        lastPongTime = Time.time;
//    }
//}


using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;

public static class NetManager
{
    public enum ServerType
    {
        Gateway,//���ط�����
        Fighter,//ս��������
    }
    private static Socket socket;
    /// <summary>
    /// �ֽ�����
    /// </summary>
    private static ByteArray byteArray;
    /// <summary>
    /// ��Ϣ�б�
    /// </summary>
    private static List<IExtensible> msgList;
    /// <summary>
    /// �Ƿ���������
    /// </summary>
    private static bool isConnecting;
    /// <summary>
    /// �Ƿ����ڹر�
    /// </summary>
    private static bool isClosing;
    /// <summary>
    /// ���Ͷ���
    /// </summary>
    private static Queue<ByteArray> writeQueue;
    /// <summary>
    /// һ֡����������Ϣ����
    /// </summary>
    private static int processMsgCount = 10;
    /// <summary>
    /// �Ƿ�������������
    /// </summary>
    private static bool isUsePing = true;
    /// <summary>
    /// ��һ�η���ping��ʱ��
    /// </summary>
    private static float lastPingTime = 0;
    /// <summary>
    /// ��һ�ν���pong��ʱ��
    /// </summary>
    private static float lastPongTime = 0;
    /// <summary>
    /// �������Ƶļ��ʱ��
    /// </summary>
    private static float pingInterval = 2f;
    /// <summary>
    /// �����¼�
    /// </summary>
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }
    /// <summary>
    /// Ҫִ�е��¼�
    /// </summary>
    /// <param name="err"></param>
    public delegate void EventListener(string err);
    /// <summary>
    /// �¼����ֵ�
    /// </summary>
    private static Dictionary<NetEvent, EventListener> eventListener = new Dictionary<NetEvent, EventListener>();
    /// <summary>
    /// ����¼�����
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] += listener;
        }
        else
        {
            eventListener.Add(netEvent, listener);
        }
    }
    /// <summary>
    /// �Ƴ��¼�����
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] -= listener;
            if (eventListener[netEvent] == null)
            {
                eventListener.Remove(netEvent);
            }
        }
    }
    /// <summary>
    /// �ַ��¼�
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="err"></param>
    public static void FireEvent(NetEvent netEvent, string err = "")
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent].Invoke(err);
        }
    }
    /// <summary>
    /// ��Ϣ����ί��
    /// </summary>
    /// <param name="msgBase"></param>
    public delegate void MsgListener(IExtensible msgBase);
    /// <summary>
    /// ��Ϣ�¼��ֵ�
    /// </summary>
    private static Dictionary<string, MsgListener> msgListener = new Dictionary<string, MsgListener>();
    /// <summary>
    /// ����¼�����
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName] += listener;
        }
        else
        {
            msgListener.Add(msgName, listener);
        }
    }
    /// <summary>
    /// �Ƴ��¼�����
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName] -= listener;
            if (msgListener[msgName] == null)
            {
                msgListener.Remove(msgName);
            }
        }
    }
    /// <summary>
    /// �ⷢ��Ϣ�¼�
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="msgBase"></param>
    public static void FireMsg(string msgName, IExtensible msgBase)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName].Invoke(msgBase);
        }
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    private static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        msgList = new List<IExtensible>();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;

        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListener.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("�Ѿ����ӣ������ظ����ӣ�");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("���������У����Ժ����ԣ�");
            return;
        }
        //��ʼ��
        Init();
        isConnecting = true;
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
            FireEvent(NetEvent.ConnectSucc);
            isConnecting = false;
            //������Ϣ
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("����ʧ��: " + e.Message);
            FireEvent(NetEvent.ConnectFail, e.Message);
            isConnecting = false;
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
            if (count == 0)
            {
                Close();
                return;
            }
            //��������
            byteArray.writeIndex += count;

            //������Ϣ
            OnReceiveData();
            //������ȹ�С������
            if (byteArray.Remain < 8)
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
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
            return;
        //��Ϣ��û�з�����
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close);
        }
    }

    /// <summary>
    /// ������յ�����
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
        string protoName = ProtobufTool.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);

        if (string.IsNullOrEmpty(protoName))
        {
            Debug.LogError("Э����Ϊ��");
            return;
        }
        byteArray.readIndex += nameCount;

        //����Э����
        int bodyLength = length - nameCount;
        IExtensible msgBase = ProtobufTool.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyLength);
        byteArray.readIndex += bodyLength;

        //�ƶ�����
        byteArray.MoveBytes();

        //��ӵ���Ϣ�б�
        lock (msgList)
        {
            msgList.Add(msgBase);
        }

        //������������
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }

    /// <summary>
    /// ����Э��
    /// </summary>
    /// <param name="msgBase"></param>
    public static void Send(IExtensible msgBase, ServerType serverType)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogError("Socketδ����");
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }
        //����
        byte[] nameBytes = ProtobufTool.EncodeName(msgBase);
        byte[] bodyBytes = ProtobufTool.Encode(msgBase);
        //��Ϣ����
        int length = nameBytes.Length + bodyBytes.Length + 1;
        //��Ϣ��
        byte[] sendBytes = new byte[2 + length];
        sendBytes[0] = (byte)(length % 256);
        sendBytes[1] = (byte)(length / 256);
        sendBytes[2] = (byte)serverType; // ��ӷ����������ֽ�
        Array.Copy(nameBytes, 0, sendBytes, 3, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 3 + nameBytes.Length, bodyBytes.Length);

        ByteArray byteArray = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(byteArray);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            socket.BeginSend(byteArray.bytes, 0, byteArray.Length, SocketFlags.None, SendCallback, socket);
        }
    }

    /// <summary>
    /// ���ͻص�����
    /// </summary>
    /// <param name="ar"></param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);

        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        ba.readIndex += count;
        //���������ϣ��Ƴ�����
        if (ba.Length == 0)
        {
            lock (writeQueue)
            {
                //���
                writeQueue.Dequeue();
                //ȡ����һ��
                ba = writeQueue.First();
            }
        }
        //����������ݣ���������
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, SocketFlags.None, SendCallback, socket);
        }
        if (isClosing)
        {
            socket.Close();
        }
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    private static void MsgUpdate()
    {
        //û����Ϣ
        if (msgList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < processMsgCount; i++)
        {
            IExtensible msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                }
            }

            if (msgBase != null)
            {
                PropertyInfo info = msgBase.GetType().GetProperty("protoName");
                string protoName = info.GetValue(msgBase).ToString();
                FireMsg(protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }

    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }
        if (Time.time - lastPingTime > pingInterval)
        {
            //����
            IExtensible msgBase = new MsgPing();
            Send(msgBase, ServerType.Gateway);
            lastPingTime = Time.time;
        }
        //�Ͽ��Ĵ���
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Debug.Log("Ping��ʱ��");
            Close();
        }
    }

    public static void Update()
    {
        PingUpdate();
        MsgUpdate();
    }

    private static void OnMsgPong(IExtensible msgBase)
    {
        lastPongTime = Time.time;
    }
}
