using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnEventConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnEventConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnEventClose);
        NetManager.Connect("127.0.0.1", 8888);
    }

    private void OnEventClose(string err)
    {
        Debug.Log("关闭连接");
    }

    private void OnEventConnectFail(string err)
    {
        Debug.Log("连接失败" + err);
    }

    private void OnEventConnectSucc(string err)
    {
        Debug.Log("连接成功");
    }
}
