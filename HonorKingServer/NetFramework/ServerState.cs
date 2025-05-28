using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
#nullable disable

/// <summary>
/// 描述客户端的对象
/// </summary>
public class ServerState
{
    /// <summary>
    /// Socket
    /// </summary>
    public Socket socket;
    /// <summary>
    /// 缓冲区
    /// </summary>
    public ByteArray readBuffer = new ByteArray();
    /// <summary>
    /// 服务器类型
    /// </summary>
    public Gateway.ServerType serverType;
}

