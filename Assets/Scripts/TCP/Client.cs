using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour {
    Socket socket;
    int port = 1234;

    byte[] readBuffer = new byte[1024];

    //当收到消息
    public Action<string> onReceiveMsg;

    //当服务器离线或被踢出
    public Action onServerOffline;

    //连接
    public void Connect(string ip, Action<bool> onComplete = null) {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var result = socket.BeginConnect(ip, port, null, null);

        Print("连接中..");

        //if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5))) {
        //    Print("timeout");
        //}

        bool success = socket.Connected;
        if (success) {
            socket.EndConnect(result);

            Print("连接成功");

        } else {
            Close();

            Print("连接异常");
        }

        onComplete?.Invoke(success);

    }

    //发送消息
    public void Send(string msg) {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(msg);
        socket.BeginSend(bytes, 0, bytes.Length, 0, SendCallback, socket);
    }
    void SendCallback(IAsyncResult ar) {
        try {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        } catch (Exception e) {
            Debug.LogError("发送异常:" + e);
        }
    }

    private void Update() {
        if (socket == null)
            return;

        //有东西可接收
        if(socket.Poll(0, SelectMode.SelectRead)) {
            try {
                byte[] readBuffer = new byte[1024];
                int count = socket.Receive(readBuffer);

                if (count <= 0) {
                    Print("服务器离线");

                    Close();

                    onServerOffline?.Invoke();

                    return;
                }

                string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

                onReceiveMsg?.Invoke(receiveStr);

                Print("收到消息: " + receiveStr);
            } catch(Exception e) {
                Debug.LogError("接收异常: " + e);

                Close();
            }
        }
    }

    void Close() {
        socket.Close();
        socket = null;
    }

    private void OnApplicationQuit() {
        if(socket != null)
            socket.Close();
    }

    void Print(object obj) {
        Debug.Log(obj.ToString());
    }
}
