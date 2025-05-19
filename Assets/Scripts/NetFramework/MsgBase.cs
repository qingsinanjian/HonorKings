using System;
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

    /// <summary>
    /// Э��������
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.protoName);
        short nameLength = (short)nameBytes.Length; 
        byte[] bytes = new byte[2 + nameLength];

        //bytes[0] = (byte)(nameLength % 256);//0000 0001 0000 0001
        //bytes[1] = (byte)(nameLength / 256);//0000 0000 1111 1111

        bytes[0] = (byte)(nameLength & 0x00FF);
        bytes[1] = (byte)(nameLength >> 8); 
        Array.Copy(nameBytes, 0, bytes, 2, nameLength);
        return bytes;
    }

    /// <summary>
    /// Э��������
    /// </summary>
    /// <param name="bytes">�ֽ�����</param>
    /// <param name="offset">��ʼλ��</param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if(offset + 2 > bytes.Length)
        {
            return "";
        }
        //short nameLength = (short)(bytes[offset + 1] * 256 + bytes[offset]);
        short nameLength = (short)((bytes[offset + 1] << 8) | bytes[offset]);
        if (nameLength <= 0)
            return "";
        count = nameLength + 2;
        return Encoding.UTF8.GetString(bytes, offset + 2, nameLength);
    }
}
