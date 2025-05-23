using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public static class ProtobufTool
{
    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="msgBase">消息</param>
    /// <returns></returns>
    public static byte[] Encode(IExtensible msgBase)
    {
        using(MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize(ms, msgBase);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IExtensible Decode(string protoName, byte[] bytes, int offset, int count)
    {
        using(MemoryStream ms = new MemoryStream(bytes, offset, count))
        {
            Type type = Type.GetType(protoName);
            if (type == null)
            {
                throw new Exception($"Type {protoName} not found.");
            }
            return (IExtensible)Serializer.NonGeneric.Deserialize(type, ms);
        }
    }

    /// <summary>
    /// 协议名编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(IExtensible msgBase)
    {
        PropertyInfo info = msgBase.GetType().GetProperty("protoName");
        string s = info.GetValue(msgBase).ToString();
        byte[] nameBytes = Encoding.UTF8.GetBytes(s);
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
        if (offset + 2 > bytes.Length)
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
