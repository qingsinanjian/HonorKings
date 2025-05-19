using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MsgBase
{
    public string protoName = "";

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);
        return Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">起始位置</param>
    /// <param name="count">长度</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = Encoding.UTF8.GetString(bytes, offset, count);
        MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, System.Type.GetType(protoName));
        return msgBase;
    }

    /// <summary>
    /// 协议名编码
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
    /// 协议名解码
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">起始位置</param>
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
