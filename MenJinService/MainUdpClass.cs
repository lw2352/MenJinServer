using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

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
        private static int maxTimeOut = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["maxTimeOut"]); //超时未响应时间--1min

        private static Hashtable htClient = new Hashtable(); //strID--DataItem

        private static Socket ServerSocket;//用于接收
        private static Socket SendSocket;//用于发送
        public static byte[] buffer = new byte[1024 + 13];//socket缓冲区

        private static int checkRecDataQueueTimeInterval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["checkRecDataQueueTimeInterval"]); // 检查接收数据包队列时间休息间隔(ms)
        //private static int checkSendDataQueueTimeInterval = 100; // 检查发送命令队列时间休息间隔(ms)
        private static int checkDataBaseQueueTimeInterval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["checkDataBaseQueueTimeInterval"]); // 检查数据库命令队列时间休息间隔(ms)

        private static int updateDataLength = 256 * 1024;//升级文件大小
        private static int maxHistoryPackage = 2 * 1024 - 256;//刷卡记录的最大包数
        
        //广播地址，255.255.255.255:6000
        private static IPEndPoint broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 6000);
        private static EndPoint broadcastRemote = (EndPoint)(broadcastIpEndPoint);

        //处理接收数据线程；ManualResetEvent:通知一个或多个正在等待的线程已发生事件
        private ManualResetEvent checkRecDataQueueResetEvent = new ManualResetEvent(true);

        //处理发送数据线程，把数据哈希表中的数据复制到各个dataItem中的发送队列
        //private ManualResetEvent checkSendDataQueueResetEvent = new ManualResetEvent(true);

        private ManualResetEvent CheckDataBaseQueueResetEvent = new ManualResetEvent(true);

        /// <summary>
        /// 初始化
        /// </summary>
        public void serviceInit()
        {
            UtilClass.utilInit();
            //byte[] a = UtilClass.hexStrToByte("789132");
            //DateTime dt = new DateTime((byte)17, 4, 1, 13, 16, 0, 0);
            MySQLDB.m_strConn = System.Configuration.ConfigurationManager.AppSettings["ServerDB"];
        }

        #region topshelf服务会调用的函数
        /// <summary>
        /// 服务开始，进行初始化
        /// </summary>
        public void Start()
        {
            serviceInit();
            CmdClass.cmdInit();
            if (OpenServer() == true)
            {
                //初始化设备状态为false
                DbClass.UpdateSensorInfo();
                UtilClass.writeLog("启动成功");
            }
            else
            {
                UtilClass.writeLog("启动失败");
            }
        }

        public void Stop()
        {
            if (CloseServer() == true)
            {
                UtilClass.writeLog("停止成功");
            }
            else
            {
                UtilClass.writeLog("停止失败");
            }
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
                //配置广播发送socket
                ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                //绑定网络地址
                ServerSocket.Bind(ipEndPoint);


                ServerSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref myRemote, new AsyncCallback(OnReceive), myRemote);

                //接收数据包处理线程
                if (!ThreadPool.QueueUserWorkItem(CheckRecDataQueue))
                    return false;
                //add3-7 合并发送数据包处理线程到接收线程中
                if (!ThreadPool.QueueUserWorkItem(CheckDataBaseQueue))
                    return false;

                IsServerOpen = true;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UtilClass.writeLog(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 关闭socket
        /// </summary>
        public bool CloseServer()
        {
            try
            {
                IsServerOpen = false;
                
                checkRecDataQueueResetEvent.WaitOne();
                //checkSendDataQueueResetEvent.WaitOne();
                CheckDataBaseQueueResetEvent.WaitOne();

                ServerSocket.Dispose();

                htClient.Clear();

                GC.SuppressFinalize(this);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UtilClass.writeLog(e.ToString());
                return false;
            }
            
        }


        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            int len = -1;
            string strID;
            byte[] id = new byte[4];
            try
            {
                EndPoint remote = (EndPoint)(ar.AsyncState);
                len = ServerSocket.EndReceiveFrom(ar, ref remote);

                //报文格式过滤
                if (buffer[0] == 0xA5 && buffer[1] == 0xA5 && buffer[len - 2] == 0x5A && buffer[len - 1] == 0x5A)
                {
                    Array.Copy(buffer, 3, id, 0, 4);
                    strID = UtilClass.byteToHexStr(id);

                    //判断哈希表中是否存在当前ID，不存在则创建，存在则把数据加入队列
                    if (htClient.ContainsKey(strID) == false)
                    {
                        DataItem dataItem = new DataItem();
                        dataItem.Init(ServerSocket, id, strID, updateDataLength, broadcastRemote,
                            maxHistoryPackage); //初始化dataItem
                        htClient.Add(strID, dataItem);
                        //把设备信息存入数据库，创建记录表
                        DbClass.addsensorinfo(strID, dataItem.HeartTime.ToString("yyyy-MM-dd HH:mm:ss"), dataItem.status.ToString());
                        DbClass.creatHistoryChildtable(strID);
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
                UtilClass.writeLog(ex.ToString());
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

                        dataItem.SendData();
                        if (CheckTimeout(dataItem.HeartTime, maxTimeOut) && dataItem.status == true)
                        {
                            dataItem.status = false;
                            //更新数据库信息
                            DbClass.UpdateSensorInfo(dataItem.strID, "status", dataItem.status.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    UtilClass.writeLog(ex.ToString());
                }
                Thread.Sleep(checkRecDataQueueTimeInterval); //当前数据处理线程休眠一段时间
            }
            checkRecDataQueueResetEvent.Set();
        }

        //读取数据库命令线程
        public void CheckDataBaseQueue(object state)
        {

            while (IsServerOpen)
            {
                try
                {
                    /*1.先读取tcommand表中所有deviceID的cmdName字段
                     * 2.“-1”表示无命令，其余值表示命令名称“name”
                     * 3.根据name来读取对应ID的operation和data
                     * 4.构造命令
                     * 5.把命令加入发送队列
                     * 6.如果是升级文件的路径，则需要读取文件内容并存入dataitem，设置标志位
                     * 7.如果是设置卡号，需要在dataitem设置大数组，加标志位并分多包发送。
                     * */
                    /*
                     *ret[i, 0] = ds1.Tables[0].Rows[i]["deviceID"].ToString();
                            ret[i, 1] = ds1.Tables[0].Rows[i]["cmdName"].ToString();
                            ret[i, 2] = ds1.Tables[0].Rows[i]["operation"].ToString();
                            ret[i, 3] = ds1.Tables[0].Rows[i]["data"].ToString();
                     */
                    string[,] cmdStrings = DbClass.readCmd();
                    if (cmdStrings != null)//先判定是否为空
                    {
                        //add 3-14,强制处理一次数据，缩减时间，避免等待
                        byte[] Cmd = new byte[13];
                        Cmd[0] = 0xA5;
                        Cmd[1] = 0xA5;
                        Cmd[2] = 0x00;
                        Cmd[3] = 0x00;
                        Cmd[4] = 0x00;
                        Cmd[5] = 0x00;
                        Cmd[6] = 0x00;
                        Cmd[7] = 0x00;
                        Cmd[8] = 0x00;
                        Cmd[9] = 0x00;
                        Cmd[10 + 0] = 0xFF;
                        Cmd[10 + 1] = 0x5A;
                        Cmd[10 + 2] = 0x5A;

                        for (int i = 0; i < cmdStrings.Length / 4; i++)
                        {
                            if (htClient.ContainsKey(cmdStrings[i, 0]))
                            {
                                DataItem dataItem = (DataItem) htClient[cmdStrings[i, 0]];
                                //有一些指令需要多包发送和读取
                                if (cmdStrings[i, 1] == "history")
                                {
                                    //先清空表的记录，再采集新纪录
                                    DbClass.deleteHistory(dataItem.strID);
                                    dataItem.tHistory.IsNeedHistory = true;
                                    //add 3-14,强制处理一次数据，缩减时间，避免等待
                                    dataItem.recDataQueue.Enqueue(Cmd); //Enqueue 将对象添加到 Queue<T> 的结尾处
                                }
                                else if (cmdStrings[i, 1] == "update")
                                {
                                        using (FileStream fsSource = new FileStream(cmdStrings[i, 3],
                                            FileMode.Open, FileAccess.Read))
                                        {
                                            // Read the source file into a byte array.
                                            for (int j = 0; j < updateDataLength; j++) //先用0xFF填充
                                            {
                                                dataItem.updateData[j] = 0xFF;
                                            }
                                            int numBytesToRead = (int)fsSource.Length;
                                            if (numBytesToRead > 0)
                                            {
                                                // Read may return anything from 0 to numBytesToRead.
                                                fsSource.Read(dataItem.updateData, 0, numBytesToRead);
                                            }

                                        }
                                    //设置升级属性
                                    dataItem.tUpdate.IsNeedUpdate = true;
                                }
                                else if (cmdStrings[i, 1] == "generalCardID")
                                {
                                    if (cmdStrings[i, 2] == "write")
                                    {
                                        dataItem.tGeneralCardId.rw = 1;
                                        dataItem.generalCardID = UtilClass.hexStrToByte(cmdStrings[i, 3]);
                                        dataItem.tGeneralCardId.IsNeedSet = true;
                                    }
                                    else if (cmdStrings[i, 2] == "read")
                                    {
                                        dataItem.tGeneralCardId.rw = 0;
                                        dataItem.tGeneralCardId.IsNeedSet = true;
                                    }
                                    //add 3-14,强制处理一次数据，缩减时间，避免等待
                                    dataItem.recDataQueue.Enqueue(Cmd); //Enqueue 将对象添加到 Queue<T> 的结尾处
                                }
                                else if (cmdStrings[i, 1] == "fingerID")
                                {
                                    if (cmdStrings[i, 2] == "write")
                                    {
                                        dataItem.tFingerId.rw = 1;
                                        dataItem.fingerID = UtilClass.hexStrToByte(cmdStrings[i, 3]);
                                        dataItem.tFingerId.IsNeedSet = true;
                                    }
                                    else if (cmdStrings[i, 2] == "read")
                                    {
                                        dataItem.tFingerId.rw = 0;
                                        dataItem.tFingerId.IsNeedSet = true;
                                    }
                                }
                                else//普通指令可以直接构造并发送
                                {                                   
                                    byte[] cmd = CmdClass.makeCommand(cmdStrings[i, 1], cmdStrings[i, 2],
                                        cmdStrings[i, 3],
                                        dataItem.byteID);
                                    if (cmd != null)
                                    {
                                        dataItem.sendDataQueue.Enqueue(cmd);
                                    }
                                }
                            }
                        }//end of for
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    UtilClass.writeLog(ex.ToString());
                }

                Thread.Sleep(checkDataBaseQueueTimeInterval);
            }

            CheckDataBaseQueueResetEvent.Set();
        }

        #endregion

        public bool CheckTimeout(DateTime heartTime, int maxSessionTimeout)
        {
            TimeSpan ts = DateTime.Now.Subtract(heartTime);
            int elapsedSecond = Math.Abs((int)ts.TotalSeconds);
            //1分钟在数据库标注
            if (elapsedSecond > maxSessionTimeout)
            {
                return true;
            }
            else return false;
        }



    }
}
