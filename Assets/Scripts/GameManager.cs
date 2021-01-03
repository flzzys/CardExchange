using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class GameManager : MonoBehaviour {
    public Button button_StartServer;
    public Button button_Start;

    public Text text;
    public Text text_Center;

    string serverIP;

    Server server;
    Client client;

    private void Awake() {
        button_StartServer.onClick.AddListener(StartServer);
        button_Start.onClick.AddListener(StartExchange);

        server = GetComponent<Server>();
        client = GetComponent<Client>();
    }

    private void Start() {
        text.text = "";

        text_Center.text = "本机IP: " + GetLocalIPv4();

        //开始接收服务器IP
        BroadcastManager.instance.StartReceiving(msg => {
            Print("接收到服务器IP: " + msg);

            serverIP = msg;

            //停止接收服务器
            BroadcastManager.instance.StopReceiving();
        });
    }

    string s = "";
    private void Update() {
        text.text = s;
    }

    #region 服务器

    //启动服务器
    void StartServer() {
        //停止接收服务器IP
        BroadcastManager.instance.StopReceiving();

        //开始广播服务器IP
        string ip = GetLocalIPv4();
        BroadcastManager.instance.StartBroadcast(ip);

        //启动服务器
        server.StartServer();

        server.onConnectToServer = () => {
            Print("接收到客户端");
        };
        //当接受到消息
        server.onReceiveMsg = msg => {
            //Print(msg);
            ClientData data = JsonConvert.DeserializeObject<ClientData>(msg);

            Print(string.Format("收到客户端: (时间:{0}, 位置:{1})", data.time, data.loc));
        };
    }

    #endregion

    #region 客户端

    enum Result { Undone, Success, Fail }

    public class ClientData {
        public Location loc;
        public DateTime time;
    }

    //开始换卡
    void StartExchange() {
        StartCoroutine(StartExchangeCor());
    }
    IEnumerator StartExchangeCor() {
        ClientData data = new ClientData();

        //当前时间
        var time = DateTime.Now;
        Debug.Log((time.AddSeconds(5) - time).TotalSeconds);
        data.time = time;

        yield return null;

        //Print("--正在获取当前位置");
        //Result result_GetPos = Result.Undone;
        ////获取位置
        //LocationService.GetPos((success, loc) => {
        //    if (success) {
        //        result_GetPos = Result.Success;

        //        //获取位置成功
        //        Print("当前位置: " + loc.ToString());

        //        data.loc = loc;

        //    } else {
        //        result_GetPos = Result.Fail;

        //        //获取位置失败
        //        Print("位置获取失败");
        //    }
        //});

        //while(result_GetPos == Result.Undone) {
        //    //Print("等待位置获取结果");
        //    yield return null;
        //}
        ////位置获取失败，结束
        //if (result_GetPos == Result.Fail) {
        //    yield break;
        //}

        Print("--正在连接到服务器");
        //连接服务器
        client.Connect(serverIP, () => {
            //连接完成
            Print("已连接到服务器");

            //发送自身位置和时间
            string dataString = JsonConvert.SerializeObject(data);
            Print(dataString);
            client.Send(dataString);
        });
        client.onReceiveMsg = msg => {
            Print("接收到消息: " + msg);
        };
    }

    #endregion

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

    void Print(object obj) {
        if(s != "") {
            s += "\n";
        }

        s += obj;
    }
}
