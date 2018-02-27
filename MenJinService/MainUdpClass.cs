using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MenJinService
{
    /// <summary>
    /// udp服务，存放主要逻辑。并且被topshelf调用
    /// </summary>
    class MainUdpClass
    {
        byte[] data = new byte[1024];
        //服务器IP
        private string ServerIP = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
        //监听端口号
        private int ServerPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);

        private static Hashtable htClient = new Hashtable(); //strID--DataItem

        private static Socket ServerSocket;

        private int checkRecDataQueueTimeInterval = 10; // 检查接收数据包队列时间休息间隔(ms)
        private int checkSendDataQueueTimeInterval = 300; // 检查发送命令队列时间休息间隔(ms)
        private int checkDataBaseQueueTimeInterval = 100; // 检查数据库命令队列时间休息间隔(ms)


        //处理接收数据线程；ManualResetEvent:通知一个或多个正在等待的线程已发生事件
        private ManualResetEvent checkRecDataQueueResetEvent = new ManualResetEvent(true);

        //处理发送数据线程，把数据哈希表中的数据复制到各个dataItem中的发送队列
        private ManualResetEvent checkSendDataQueueResetEvent = new ManualResetEvent(true);

        private ManualResetEvent CheckDataBaseQueueResetEvent = new ManualResetEvent(true);

#region topshelf服务会调用的函数
        /// <summary>
        /// 服务开始，进行初始化
        /// </summary>
        public void Start()
        {
            UtilClass.initUtil();
            UtilClass.writeLog("已启动");
        }

        public void Stop()
        {
            UtilClass.writeLog("已停止");
        }

        public void Continued() { UtilClass.writeLog("继续运行"); }
        public void Paused() { UtilClass.writeLog("已暂停"); }
        public void Shutdown() { UtilClass.writeLog("已卸载"); }

#endregion

        /// <summary>
        /// 开启socket服务
        /// </summary>
        /// <returns></returns>
        private bool OpenServer()
        {
            try
            {
                IPEndPoint myclient = new IPEndPoint(IPAddress.Any, 0);
                EndPoint myRemote = (EndPoint)(myclient);

                //得到本机IP，设置TCP端口号         
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //绑定网络地址
                ServerSocket.Bind(ipEndPoint);

                ServerSocket.BeginReceiveFrom(data, 0, data.Length, SocketFlags.None, ref myRemote, new AsyncCallback(OnReceive), myRemote);

                //接收数据包处理线程
                /*if (!ThreadPool.QueueUserWorkItem(CheckRecDataQueue))
                    return false;
                //发送数据包处理线程
                if (!ThreadPool.QueueUserWorkItem(CheckSendDataQueue))
                    return false;
                if (!ThreadPool.QueueUserWorkItem(CheckDataBaseQueue))
                    return false;*/

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// 关闭socket
        /// </summary>
        public void CloseServer()
        {
            ServerSocket.Dispose();
        }


        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            int len = -1;
            try
            {
                EndPoint remote = (EndPoint)(ar.AsyncState);
                len = ServerSocket.EndReceiveFrom(ar, ref remote);

                //继续接收数据
                ServerSocket.BeginReceiveFrom(data, 0, data.Length, SocketFlags.None, ref remote, new AsyncCallback(OnReceive), remote);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        


    }
}
