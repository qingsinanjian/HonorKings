﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
#nullable disable

/// <summary>
/// 描述客户端的对象
/// </summary>
public class ClientState
{
    /// <summary>
    /// 客户端Socket
    /// </summary>
    public Socket socket;
    /// <summary>
    /// 客户端的缓冲区
    /// </summary>
    public ByteArray readBuffer = new ByteArray();
    /// <summary>
    /// 上一次收到ping的时间
    /// </summary>
    public long lastPingTime = 0;
}

