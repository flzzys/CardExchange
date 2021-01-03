using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Security.Permissions;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace CardExchangeServer {
    class ClientInfo {
        public Socket socket;
        public byte[] readBuffer = new byte[1024];
    }

    //位置，经纬度高度
    public class Location {
        //纬度
        public float latitude;
        //经度
        public float longitude;
        //高度
        public float altitude;

        public override string ToString() {
            return string.Format("({0}, {1})", latitude, longitude);
        }
    }

    //用户数据
    public class ClientData {
        public Location loc;
        public DateTime time;
    }

    class Program {
        //服务器
        Socket server;
        int port = 1234;

        //用户列表
        List<ClientInfo> clientInfoList = new List<ClientInfo>();

        static Program instance;

        //开始
        static void Main(string[] args) {
            instance = new Program();
            instance.Start();
        }

        void Start() {
            //开始广播
            string ip = GetLocalIPv4();
            instance.StartBroadcast(ip);

            //启动服务器
            instance.StartServer();

            //开始自动Update
            Thread update = new Thread(new ThreadStart(instance.StartUpdating));
            update.Start();
        }

        void StartUpdating() {
            while (true) {
                Update();

                Thread.Sleep(1000);
            }
        }

        private void Update() {
            //接收信息
            UpdateReceiveMsg();
        }


        #region 服务器相关

        //启动服务器
        public void StartServer() {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(GetLocalIPv4());
            IPEndPoint iep = new IPEndPoint(ip, port);
            server.Bind(iep);
            server.Listen(0);

            Console.WriteLine(string.Format("服务器启动! (IP: {0})", GetLocalIPv4()));

            //开始监听
            server.BeginAccept(AcceptCallback, server);
        }

        //接收客户端
        void AcceptCallback(IAsyncResult ar) {
            try {
                Socket server = (Socket)ar.AsyncState;
                Socket client = server.EndAccept(ar);

                //新增客户信息
                ClientInfo info = new ClientInfo() { socket = client };
                clientInfoList.Add(info);

                Console.WriteLine(string.Format(string.Format("{0}已加入", GetIP(client))));

                OnReceiveClient(client);

                //继续监听
                server.BeginAccept(AcceptCallback, server);

            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        //接收信息
        void UpdateReceiveMsg() {
            foreach (var info in clientInfoList) {
                Socket client = info.socket;

                if (client == null)
                    return;

                //有东西可接收
                if (client.Poll(0, SelectMode.SelectRead)) {
                    byte[] readBuffer = new byte[1024];
                    int count = client.Receive(readBuffer);

                    string ip = GetIP(client);

                    //收到信息小于等于0，代表客户端关闭
                    if (count <= 0) {
                        clientInfoList.Remove(info);
                        info.socket.Close();

                        Console.WriteLine(string.Format(string.Format("{0}已离线", ip)));

                        return;
                    }

                    string receiveStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

                    OnReceiveMsg(info, receiveStr);
                }
            }
        }

        #endregion

        #region API

        //当接收客户端
        void OnReceiveClient(Socket client) {

        }

        //当收到消息
        void OnReceiveMsg(ClientInfo info, string msg) {
            //转换为客户端数据
            ClientData data = JsonConvert.DeserializeObject<ClientData>(msg);
            Console.WriteLine(data.time);

            //遍历客户端列表，找出位置接近的，互相发卡，重置剩余时间
        }

        #endregion

        #region 广播

        const int udpPort = 12345;

        Socket broadcaster;
        IPEndPoint iep;
        byte[] data;

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

        //停止广播
        public void StopBroadcasting() {
            if (broadcaster != null) {
                broadcaster.Close();
                broadcaster = null;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------

        //获取IP地址
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

        //获取Socket的IP
        string GetIP(Socket socket) {
            return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
        }
    }
}
