using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

//用户数据
public class ClientData {
    public string msg;
    public string color;
    public Location loc;
    public DateTime time;
}

//服务器发回数据
public class ServerData {
    public string client;
    public string msg;
    public string color;
    public float distance;
}

public class GameManager : MonoBehaviour {
    public Button button_Start;
    public Button button_Clear;

    public Text text;
    public Text text_Center;
    public InputField inputField;
    public Image image_CatThing;

    const string serverIP = "49.235.226.171";
    //const string serverIP = "192.168.31.196";

    Server server;
    Client client;

    private void Awake() {
        server = GetComponent<Server>();
        client = GetComponent<Client>();
    }

    private void Start() {
        text.text = "";
        text_Center.text = "本机IP:" + GetLocalIPv4();

        //获取随机颜色
        playerColor = colors[UnityEngine.Random.Range(0, colors.Length)];
        image_CatThing.color = playerColor;

        button_Clear.onClick.AddListener(() => {
            s = "";
        });

        //如果未启动位置服务
        if (!LocationService.enableLocationService) {
            button_Start.GetComponentInChildren<Text>().text = "启动位置服务";
            button_Start.onClick.AddListener(CheckLocationService);
        } else {
            button_Start.onClick.AddListener(StartExchange);
        }

        //开始接收服务器IP
        //BroadcastManager.instance.StartReceiving(msg => {
        //    Print("接收到服务器IP: " + msg);

        //    button_Start.interactable = true;

        //    serverIP = msg;

        //    //停止接收服务器
        //    BroadcastManager.instance.StopReceiving();
        //});

    }

    string s = "";
    private void Update() {
        text.text = s;
    }

    #region 客户端

    enum Result { Undone, Success, Fail }

    //开始换卡
    void StartExchange() {
        StartCoroutine(StartExchangeCor());
    }
    IEnumerator StartExchangeCor() {
        ClientData data = new ClientData();

        //设置颜色
        data.color = ConvertColorToString(playerColor);

        //获取输入框文本
        string str = inputField.text;
        if(str == "") {
            str = inputField.placeholder.GetComponent<Text>().text;
        }
        data.msg = str;

        //当前时间
        var time = DateTime.Now;
        Debug.Log((time.AddSeconds(5) - time).TotalSeconds);
        data.time = time;

        yield return null;

        Print("--正在获取当前位置");
        Result result_GetPos = Result.Undone;
        //获取位置
        LocationService.GetPos((success, loc) => {
            if (success) {
                result_GetPos = Result.Success;

                //获取位置成功
                Print("当前位置: " + loc.ToString());

                data.loc = loc;

            } else {
                result_GetPos = Result.Fail;

                //获取位置失败
                Print("位置获取失败");
            }
        });

        while (result_GetPos == Result.Undone) {
            //Print("等待位置获取结果");
            yield return null;
        }
        //位置获取失败，结束
        if (result_GetPos == Result.Fail) {
            yield break;
        }

        Print("--正在连接到服务器");
        //连接服务器
        client.Connect(serverIP, () => {
            //连接完成
            Print("已连接到服务器");
            Print("----------------");

            //发送自身位置和时间
            string dataString = JsonConvert.SerializeObject(data);
            //Print(dataString);
            client.Send(dataString);
        });
        client.onReceiveMsg = OnReceiveMsg;
        client.onServerOffline = () => {
            Print("换卡结束");
            Print("----------------");
        };
    }

    //当客户端接收到消息
    void OnReceiveMsg(string msg) {
        //Print("接收到消息: " + msg);

        foreach (var item in msg.Split('|')) {
            ServerData serverData = JsonConvert.DeserializeObject<ServerData>(item);
            Print(string.Format("<color={0}>{1}(距离{2}米)</color>: {3}", serverData.color, serverData.client, serverData.distance.ToString("f0"), serverData.msg));
        }
        

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


    #region 颜色

    Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan, Color.yellow, Color.magenta };
    Color playerColor;

    //颜色和字符串转换
    Color ConverStringToColor(string str) {
        Color color;
        ColorUtility.TryParseHtmlString(str, out color);
        return color;
    }
    string ConvertColorToString(Color color) {
        return "#" + ColorUtility.ToHtmlStringRGBA(color);
    }

    #endregion

    void CheckLocationService() {
        Print("正在尝试获取位置服务..");
        LocationService.instance.StartService(isSuccess => {
            if (isSuccess) {
                Print("位置服务已启动!");
                button_Start.GetComponent<Animator>().SetTrigger("Flip");

                StartCoroutine(Delay());

                button_Start.onClick.RemoveAllListeners();
                button_Start.onClick.AddListener(StartExchange);
            } else {
                Print("不支持位置服务:(");
            }
        });
    }

    IEnumerator Delay() {
        button_Start.interactable = false;
        yield return new WaitForSeconds(.33f);
        button_Start.interactable = true;

        button_Start.GetComponentInChildren<Text>().text = "开始换卡";
    }
}
