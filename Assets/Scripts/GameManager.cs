using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class GameManager : MonoBehaviour {
    private void Start() {
        
    }

    private void Update() {
        if (Input.GetKeyDown("1")) {
            Server server = GetComponent<Server>();
            server.StartServer();
        }
        if (Input.GetKeyDown("2")) {
            Client client = GetComponent<Client>();
            //client.Connect("127.0.0.1");
            client.Connect("192.168.31.196");
        }
    }

    //进入换卡界面

    //开始换卡
    void StartExchange() {
        //上传位置、当前时间到服务器
        //时间
        var time = System.DateTime.Now;
        Debug.Log(time);

        //位置

        //开始接收信息


    }

    public static string GetLocalIPv4() {
        string hostName = Dns.GetHostName();
        IPHostEntry iPEntry = Dns.GetHostEntry(hostName);
        for (int i = 0; i < iPEntry.AddressList.Length; i++) {
            //从IP地址列表中筛选出IPv4类型的IP地址
            if (iPEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                return iPEntry.AddressList[i].ToString();
        }
        return null;
    }
}
