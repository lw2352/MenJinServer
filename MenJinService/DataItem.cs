using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MenJinService
{
    //历史结构体
    struct History
    {
        public bool IsNeedHistory;//是否需要升级
        public int currentNum;
    };
    //升级结构体
    struct Update
    {
        public bool IsNeedUpdate;//是否需要升级
        public int currentNum;
    };

    //设备类
    class DataItem
    {
        public string strID;//设备ID
        public byte[] byteID;
        public bool status;//在线状态
        public History tHistory;
        public Update tUpdate;

        public Socket socket;//实际共用SendSocket，用来发送数据
        public EndPoint remote;//客户端节点，实际用广播包
        public DateTime HeartTime; //上一次心跳包发上来的时间
        public byte[] updateData; //所有数据,存放历史记录
        public int maxHistoryPackage;//刷卡记录的最大包数
        public Queue<byte[]> recDataQueue = new Queue<byte[]>();//数据接收队列；queue是对象的先进先出集合
        public Queue<byte[]> sendDataQueue = new Queue<byte[]>();//数据发送队列
        private delegate void AsyncAnalyzeData(byte[] data);

        /// <summary>
        /// 初始化DataItem
        /// </summary>
        public void Init(Socket serverSocket, byte[] id, string strid, int updateDataLength, EndPoint broadcastEndPoint, int maxNum)
        {
            socket = serverSocket;
            strID = strid;
            byteID = id;
            status = true;
            updateData = new byte[updateDataLength];
            HeartTime = DateTime.Now;

            remote = broadcastEndPoint;
            maxHistoryPackage = maxNum;
            //other
            tHistory.IsNeedHistory = false;
            tHistory.currentNum = 0;
            tUpdate.IsNeedUpdate = false;
            tUpdate.currentNum = 0;
        }

        public void HandleData()
        {
            if (recDataQueue.Count > 0 && status == true)//命令已发送后，得到返回信息需要一段时间，再去解析数据
            {
                byte[] datagramBytes = recDataQueue.Dequeue();//读取 Queue<T> 开始处的对象并移除
                AsyncAnalyzeData method = new AsyncAnalyzeData(AnalyzeData);
                method.BeginInvoke(datagramBytes, null, null);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void SendData()
        {
            if (sendDataQueue.Count > 0 && status == true)//没有待解析的命令，可以去发送命令
            {
                byte[] datagramBytes = sendDataQueue.Dequeue(); //读取 Queue<T> 开始处的对象并移除
                SendCmd(datagramBytes);       
            }
        }

        //处理数据和写入数据库
        public void AnalyzeData(byte[] datagramBytes)
        {
            string msg;
            try
            {
                switch (datagramBytes[2])
                {
                    case 0x00:
                        status = true;
                        HeartTime = DateTime.Now;
                        DbClass.UpdateSensorInfo(strID, "lastLoginTime", HeartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        break;

                    case 0x23:
                        if (tHistory.IsNeedHistory == true)
                        {
                            tHistory.currentNum++;//当前读取包数加1
                            //解析数据
                            string[,] dadaStrings = new string[1024,3];
                            int dataNum=0;
                            for (int i = 0; i < 1024; i+=8)
                            {
                                if (datagramBytes[10 + i+3] != 0xFF)//年份不可能为0xFF，否则是没有数据
                                {
                                    
                                    //卡号
                                    dadaStrings[dataNum, 0] = UtilClass.hex2String[datagramBytes[10 + i]] +
                                                        UtilClass.hex2String[datagramBytes[10 + i + 1]] +
                                                        UtilClass.hex2String[datagramBytes[10 + i + 2]];
                                    //时间
                                    /*dadaStrings[dataNum, 1] = UtilClass.hex2String[datagramBytes[10 + i + 3]] + //年
                                                        UtilClass.hex2String
                                                            [datagramBytes[10 + i + 4] & 0x0F] + //低四位表示月
                                                        UtilClass.hex2String[datagramBytes[10 + i + 5]] + //日
                                                        UtilClass.hex2String[datagramBytes[10 + i + 6]] + //时
                                                        UtilClass.hex2String[datagramBytes[10 + i + 7]]; //分*/
                                    DateTime dt = new DateTime(datagramBytes[10 + i + 3]+2000, datagramBytes[10 + i + 4] & 0x0F, datagramBytes[10 + i + 5], datagramBytes[10 + i + 6], datagramBytes[10 + i + 7], 0, 0);
                                    dadaStrings[dataNum, 1] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                    //门号, 高四位表示门号
                                    if ((datagramBytes[10 + i + 4] >>4) == 0x00)
                                    {
                                        dadaStrings[dataNum, 2] = "A";
                                    }
                                    else if ((datagramBytes[10 + i + 4] >>4) == 0x01)
                                    {
                                        dadaStrings[dataNum, 2] = "B";
                                    }
                                    dataNum++;//有效记录数
                                }
                                else//停止读取,复位结构体成员
                                {
                                    tHistory.IsNeedHistory = false;
                                    tHistory.currentNum = 0;
                                    //跳出for循环
                                    break;
                                }
                            }//end of for
                            //写入数据库
                            DbClass.insertHistory(strID, dadaStrings, dataNum);
                        }
                        break;

                    default:
                        break;                        
                }

                if (tHistory.IsNeedHistory == true)
                {
                    SendCmd(SetHisCmd(tHistory.currentNum));
                }
                if (tUpdate.IsNeedUpdate == true)
                {
                    SendCmd(SetUpdateCmd(datagramBytes));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "硬件"+strID+"收到数据："+UtilClass.byteToHexStr(datagramBytes));
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        private void SendCmd(byte[] cmd)
        {
            try
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "硬件" + strID + "发送数据：" + UtilClass.byteToHexStr(cmd));
                socket.BeginSendTo(cmd, 0, cmd.Length, SocketFlags.None, remote, new AsyncCallback(OnSend), socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSendTo(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private byte[] SetHisCmd(int bulkCount)
        {
            /***************************************************************************7-读写位; 8，9第几包; 10-数据位*************************************/
            byte[] Cmd = { 0xA5, 0xA5, 0x23, byteID[0], byteID[1], byteID[2], byteID[3], 0x00, 0x00, 0x00, 0x00, 0xFF, 0x5A, 0x5A };
            byte[] bytesbulkCount = new byte[2];
            bytesbulkCount = intToBytes(bulkCount);

            Cmd[8] = bytesbulkCount[0];
            Cmd[9] = bytesbulkCount[1];

            return (Cmd);
        }

        private byte[] SetUpdateCmd(byte[] data)
        {
            /***************************************************************************7-读写位*8，9第几包**10-数据位*************************************/
            byte[] Cmd = { 0xA5, 0xA5, 0x23, byteID[0], byteID[1], byteID[2], byteID[3], 0x00, 0x00, 0x00, 0x00, 0xFF, 0x5A, 0x5A };
            byte[] bytesbulkCount = new byte[2];
            //bytesbulkCount = intToBytes(bulkCount);

            Cmd[8] = bytesbulkCount[0];
            Cmd[9] = bytesbulkCount[1];

            return (Cmd);
        }

        /// <summary>
        /// 将int数值转换为占byte数组
        /// </summary>
        /// <param name="value">int</param>
        /// <returns>byte[]</returns>
        private byte[] intToBytes(int value)
        {
            byte[] src = new byte[2];

            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }

    }
}
