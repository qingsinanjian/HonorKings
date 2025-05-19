using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MsgBase
{
    public string protoName = "";

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);
        return Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="protoName">Э����</param>
    /// <param name="bytes">�ֽ�����</param>
    /// <param name="offset">��ʼλ��</param>
    /// <param name="count">����</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = Encoding.UTF8.GetString(bytes, offset, count);
        MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, System.Type.GetType(protoName));
        return msgBase;
    }
}
