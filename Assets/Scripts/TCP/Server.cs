using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

class ClientInfo {
    public Socket socket;
    public byte[] readBuffer = new byte[1024];
}

public class Server : MonoBehaviour {
    Socket server;
    int port = 1234;

    List<ClientInfo> clientInfoList = new List<ClientInfo>();

    //当连接到服务器
    public Action onConnectToServer;
    //当收到消息
    public Action<string> onReceiveMsg;

    //启动服务器
    public void StartServer() {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse(GameManager.GetLocalIPv4());
        IPEndPoint iep = new IPEndPoint(ip, port);
        server.Bind(iep);
        server.Listen(0);

        Print("服务器启动");

        //开始监听
        server.BeginAccept(AcceptCallback, server);
    }

    //停止服务器
    public void StopServer() {
        if(server != null) {
            server.Close();
            server = null;
        }
    }

    //接收客户端
    void AcceptCallback(IAsyncResult ar) {
        try {
            Socket server = (Socket)ar.AsyncState;
            Socket client = server.EndAccept(ar);

            //新增客户信息
            ClientInfo info = new ClientInfo() { socket = client };
            clientInfoList.Add(info);

            //Broadcast(string.Format(string.Format("{0}已加入", GetIP(client))));

            onConnectToServer?.Invoke();

            //继续监听
            server.BeginAccept(AcceptCallback, server);

        } catch (Exception e) {
            Debug.LogError(e);
        }
    }

    //发送消息
    void Send(Socket socket, string msg) {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(msg);
        socket.Send(bytes);
    }

    //广播给所有客户端
    void Broadcast(string msg) {
        foreach (var info in clientInfoList) {
            Send(info.socket, msg);
        }
    }

    string GetIP(Socket socket) {
        return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
    }

    private void Update() {
        foreach (var info in clientInfoList) {
            Socket socket = info.socket; 
            
            if (socket == null)
                return;

            //有东西可接收
            if (socket.Poll(0, SelectMode.SelectRead)) {
                byte[] readBuffer = new byte[1024];
                int count = socket.Receive(readBuffer);

                string ip = GetIP(socket);

                //收到信息小于等于0，代表客户端关闭
                if (count <= 0) {
                    clientInfoList.Remove(info);
                    info.socket.Close();

                    //Broadcast(string.Format(string.Format("{0}已离线", ip)));

                    return;
                }

                string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

                //string msg = string.Format("<color=#{0}>{1}：</color>{2}", ColorUtility.ToHtmlStringRGBA(Color.red), ip, receiveStr);
                //Broadcast(msg);

                onReceiveMsg?.Invoke(receiveStr);
            }
        }
    }


    private void OnApplicationQuit() {
        if (server != null) {
            server.Close();
            server = null;
        }

        foreach (var info in clientInfoList) {
            Socket socket = info.socket;
            if (socket != null) {
                socket.Close();
            }
        }
    }

    void Print(object obj) {
        Debug.Log(obj.ToString());
    }
}
