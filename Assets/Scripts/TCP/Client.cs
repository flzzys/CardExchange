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

    //连接
    public void Connect(string ip) {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(ip, port, ConnectCallback, socket);

        Print("连接");
    }
    void ConnectCallback(IAsyncResult ar) {
        try {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Print("连接成功");
        } catch (Exception e) {
            Debug.LogError(e);
        }
    }
    
    //接收消息
    void ReceiveCallback(IAsyncResult ar) {
        try {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);

            if (count <= 0) {
                Print("服务器离线");

                return;
            }

            string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

            Print("收到消息：" + receiveStr);

            socket.BeginReceive(readBuffer, 0, 1024, 0, ReceiveCallback, socket);

        } catch (Exception e) {
            Debug.LogError(e);
        }
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
            Debug.LogError(e);
        }
    }

    private void Update() {
        if (socket == null)
            return;

        //有东西可接收
        if(socket.Poll(0, SelectMode.SelectRead)) {
            byte[] readBuffer = new byte[1024];
            int count = socket.Receive(readBuffer);

            if (count <= 0)
                return;

            string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

            Print("收到消息: " + receiveStr);
        }
    }

    private void OnApplicationQuit() {
        if(socket != null)
            socket.Close();
    }

    void Print(object obj) {
        Debug.Log(obj.ToString());
    }
}
