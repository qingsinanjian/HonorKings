using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgHandler
{
    //public static void MsgPing(ClientState state, MsgBase msgBase)
    //{
    //    Console.WriteLine("MsgPing:" + state.socket.RemoteEndPoint);
    //    state.lastPingTime = NetManager.GetTimeStamp();
    //    MsgPong msgPong = new MsgPong();
    //    NetManager.Send(state, msgPong);
    //}

    public static void MsgPing(ClientState state, IExtensible msgBase)
    {
        Console.WriteLine("MsgPing:" + state.socket.RemoteEndPoint);
        state.lastPingTime = NetManager.GetTimeStamp();
        MsgPong msgPong = new MsgPong();
        NetManager.Send(state, msgPong);
    }
}
