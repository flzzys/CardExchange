using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace CardExchangeServer {
    class ClientInfo {
        public Socket socket;
        public byte[] readBuffer = new byte[1024];
    }

    class Program {
        //服务器
        Socket server;
        int port = 1234;

        //用户列表
        List<ClientInfo> clientInfoList = new List<ClientInfo>();

        //开始
        static void Main(string[] args) {
            string ip = GetLocalIPv4();

            //开始广播
            Program p = new Program();
            p.StartBroadcast(ip);

            //启动服务器
            p.StartServer();

            Console.Read();
        }

        //启动服务器
        public void StartServer() {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(GetLocalIPv4());
            IPEndPoint iep = new IPEndPoint(ip, port);
            server.Bind(iep);
            server.Listen(0);

            Console.WriteLine("服务器启动!");

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

                //继续监听
                server.BeginAccept(AcceptCallback, server);

            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

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
