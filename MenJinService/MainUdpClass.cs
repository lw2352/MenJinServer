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
        public bool IsServerOpen;
        //服务器IP
        private static string ServerIP = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
        //监听端口号
        private static int ServerPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);

        private static Hashtable htClient = new Hashtable(); //strID--DataItem

        private static Socket ServerSocket;
        public static byte[] buffer = new byte[1024 + 13];//socket缓冲区

        private static int checkRecDataQueueTimeInterval = 10; // 检查接收数据包队列时间休息间隔(ms)
        private static int checkSendDataQueueTimeInterval = 300; // 检查发送命令队列时间休息间隔(ms)
        private static int checkDataBaseQueueTimeInterval = 100; // 检查数据库命令队列时间休息间隔(ms)

        private static int updateDataLength = 256 * 1024;//升级文件大小
        private static int maxHistoryPackage = 2 * 1024 - 256;//刷卡记录的最大包数

        private static int maxTimeOut = 60; //超时未响应时间--1min
        //广播地址，255.255.255.255:6000
        private static IPEndPoint broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 6000);
        private static EndPoint broadcastRemote = (EndPoint)(broadcastIpEndPoint);

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

                ServerSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref myRemote, new AsyncCallback(OnReceive), myRemote);

                //接收数据包处理线程
                /*if (!ThreadPool.QueueUserWorkItem(CheckRecDataQueue))
                    return false;
                //发送数据包处理线程
                if (!ThreadPool.QueueUserWorkItem(CheckSendDataQueue))
                    return false;
                if (!ThreadPool.QueueUserWorkItem(CheckDataBaseQueue))
                    return false;*/

                IsServerOpen = true;

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
            IsServerOpen = false;
        }


        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            int len = -1;
            string strID;
            try
            {
                EndPoint remote = (EndPoint)(ar.AsyncState);
                len = ServerSocket.EndReceiveFrom(ar, ref remote);

                //报文格式过滤
                if (buffer[0] == 0xA5 && buffer[1] == 0xA5 && buffer[len - 2] == 0x5A && buffer[len - 1] == 0x5A)
                {
                    strID = UtilClass.hex2String[buffer[3]].str +
                            UtilClass.hex2String[buffer[4]].str +
                            UtilClass.hex2String[buffer[5]].str +
                            UtilClass.hex2String[buffer[6]].str;

                    //判断哈希表中是否存在当前ID
                    if (htClient.ContainsKey(strID) == false)
                    {
                        DataItem dataItem = new DataItem();
                        dataItem.Init(ServerSocket, strID, updateDataLength, broadcastRemote,
                            maxHistoryPackage); //初始化dataItem
                        htClient.Add(strID, dataItem);
                        //TODO:把设备信息存入数据库，创建记录表
                    }
                    else
                    {
                        DataItem dataItem = (DataItem)htClient[strID]; //取出address对应的dataitem
                        byte[] recData = new byte[len];
                        Array.Copy(buffer, recData, len);
                        dataItem.recDataQueue.Enqueue(recData); //Enqueue 将对象添加到 Queue<T> 的结尾处
                    }


                }

                //继续接收数据
                ServerSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remote, new AsyncCallback(OnReceive), remote);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

#region 任务
        //处理接收队列
        private void CheckRecDataQueue(object state)
        {
            checkRecDataQueueResetEvent.Reset(); //Reset()将事件状态设置为非终止状态，导致线程阻止。
            while (IsServerOpen)
            {
                try
                {
                    foreach (DataItem dataItem in htClient.Values)
                    {
                        dataItem.HandleData();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                Thread.Sleep(checkRecDataQueueTimeInterval); //当前数据处理线程休眠一段时间
            }
            checkRecDataQueueResetEvent.Set();
        }

        //发送队列里面的命令
        private void CheckSendDataQueue(object state)
        {
            checkSendDataQueueResetEvent.Reset(); //Reset()将事件状态设置为非终止状态，导致线程阻止。
            while (IsServerOpen)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                Thread.Sleep(checkSendDataQueueTimeInterval); //当前数据处理线程休眠一段时间
            }
            checkSendDataQueueResetEvent.Set();
        }

        //读取数据库命令线程
        public void CheckDataBaseQueue(object state)
        {

            while (IsServerOpen)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }

                Thread.Sleep(checkDataBaseQueueTimeInterval);
            }
        }



        #endregion




    }
}
