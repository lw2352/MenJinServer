using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MenJinService
{
    //设备类
    class DataItem
    {
        public string strID;//设备ID
        public byte[] byteID;
        public bool status;//在线状态
        public Socket socket;//实际共用serverSocket，用来发送数据
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

                    default:
                        break;
                        
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
                socket.BeginSendTo(cmd, 0, cmd.Length, SocketFlags.None, remote, new AsyncCallback(OnSend), this);
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

    }
}
