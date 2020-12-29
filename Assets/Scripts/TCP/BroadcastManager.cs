using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

//在局域网广播和接收消息
public class BroadcastManager : Singleton<BroadcastManager> {
    const int udpPort = 12345;

    Socket broadcaster, receiver;
    IPEndPoint iep;
    byte[] data;

    Action<string> onReceivedMsg;

    private void Update() {
        if (receiver == null)
            return;

        if (receiver.Poll(0, SelectMode.SelectRead)) {
            byte[] readBuffer = new byte[1024];
            int count = receiver.Receive(readBuffer);
            string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

            //print("接收到消息" + receiveStr);
            onReceivedMsg(receiveStr);
        }
    }

    //开始广播IP
    public void StartBroadcast(string msg) {
        broadcaster = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        broadcaster.EnableBroadcast = true;
        iep = new IPEndPoint(IPAddress.Broadcast, udpPort);
        broadcaster.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

        data = System.Text.Encoding.ASCII.GetBytes(msg);

        Thread thread = new Thread(new ThreadStart(Brocast));
        thread.Start();
    }
    //每秒向局域网内广播一次
    void Brocast() {
        while (broadcaster != null) {
            broadcaster.SendTo(data, iep);

            Thread.Sleep(1000);
        }
    }

    public void StopBroadcasting() {
        if(broadcaster != null) {
            broadcaster.Close();
            broadcaster = null;
        }
    }

    //开始接收广播
    public void StartReceiving(Action<string> onReceivedMsg = null) {
        this.onReceivedMsg = onReceivedMsg;

        receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.EnableBroadcast = true;
        iep = new IPEndPoint(IPAddress.Any, udpPort);
        receiver.Bind(iep);
    }

    //停止接收广播
    public void StopReceiving() {
        if (receiver != null) {
            receiver.Close();

            receiver = null;
        }
    }

    //自动关闭
    private void OnApplicationQuit() {
        if(receiver != null) {
            receiver.Close();
        }

        if (broadcaster != null) {
            broadcaster.Close();
        }
    }
}
