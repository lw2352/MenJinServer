using System;
using System.CodeDom;
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

    //普通卡号结构体
    struct GeneralCardId
    {
        public bool IsNeedSet;//是否需要
        public int currentNum;
        public byte rw;//读写
    };

    //指纹卡号结构体
    struct FingerId
    {
        public bool IsNeedSet;//是否需要
        public int currentNum;
        public byte rw;//读写
    };

    //设备类
    class DataItem
    {
        public string strID;//设备ID
        public byte[] byteID;
        public bool status;//在线状态
        public History tHistory;
        public Update tUpdate;

        public GeneralCardId tGeneralCardId;
        public FingerId tFingerId;

        public Socket socket;//实际共用SendSocket，用来发送数据
        public EndPoint remote;//客户端节点，实际用广播包
        public DateTime HeartTime; //上一次心跳包发上来的时间
        public byte[] updateData; //所有数据,存放历史记录
        public byte[] generalCardID = new byte[1500];//普通卡
        public byte[] fingerID = new byte[1500];//指纹ID
        public int maxHistoryPackage;//刷卡记录的最大包数
        public Queue<byte[]> recDataQueue = new Queue<byte[]>();//数据接收队列；queue是对象的先进先出集合
        public Queue<byte[]> sendDataQueue = new Queue<byte[]>();//数据发送队列
        //private delegate void AsyncAnalyzeData(byte[] data);

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
            if (recDataQueue.Count > 0)//命令已发送后，得到返回信息需要一段时间，再去解析数据
            {
                byte[] datagramBytes = recDataQueue.Dequeue();//读取 Queue<T> 开始处的对象并移除
                //AsyncAnalyzeData method = new AsyncAnalyzeData(AnalyzeData);
                //method.BeginInvoke(datagramBytes, null, null);
                AnalyzeData(datagramBytes);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void SendData()
        {
            if (sendDataQueue.Count > 0)//没有待解析的命令，可以去发送命令
            {
                byte[] datagramBytes = sendDataQueue.Dequeue(); //读取 Queue<T> 开始处的对象并移除
                SendCmd(datagramBytes);       
            }
        }

        //处理数据和写入数据库
        public void AnalyzeData(byte[] datagramBytes)
        {
            try
            {
                switch (datagramBytes[2])
                {
                    #region 心跳包和刷卡记录
                    //心跳包（也用于搜索设备）
                    case 0x00:
                        status = true;
                        HeartTime = DateTime.Now;
                        DbClass.UpdateSensorInfo(strID, "lastLoginTime", HeartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        DbClass.UpdateSensorInfo(strID, "status", status.ToString());
                        break;

                    //刷卡记录
                    case 0x23:
                        if (tHistory.IsNeedHistory == true)
                        {
                            if (datagramBytes[10] == 0xAA)
                            {
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "fail");
                                break;
                            }

                            tHistory.currentNum++;//当前读取包数加1
                            //写入数据库,历史记录的包太多，除以8，避免超出byte范围
                            if (tHistory.currentNum % 8 == 0)
                            {
                                DbClass.UpdateCmd(strID, "data", UtilClass.hex2String[tHistory.currentNum / 8]);
                            }

                            if (tHistory.currentNum == maxHistoryPackage)//读到尾部了
                            {
                                tHistory.IsNeedHistory = false;
                                tHistory.currentNum = 0;
                            }
                            //解析数据
                            string[,] dadaStrings = new string[1024, 3];
                            int dataNum = 0;
                            for (int i = 0; i < 1024; i += 8)
                            {
                                if (datagramBytes[10 + i + 3] != 0xFF)//年份不可能为0xFF，否则是没有数据
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
                                    DateTime dt = new DateTime(datagramBytes[10 + i + 3] + 2000, datagramBytes[10 + i + 4] & 0x0F, datagramBytes[10 + i + 5], datagramBytes[10 + i + 6], datagramBytes[10 + i + 7], 0, 0);
                                    dadaStrings[dataNum, 1] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                    //门号, 高四位表示门号
                                    if ((datagramBytes[10 + i + 4] >> 4) == 0x00)
                                    {
                                        dadaStrings[dataNum, 2] = "A";
                                    }
                                    else if ((datagramBytes[10 + i + 4] >> 4) == 0x01)
                                    {
                                        dadaStrings[dataNum, 2] = "B";
                                    }
                                    dataNum++;//有效记录数
                                    //写入数据库
                                    DbClass.insertHistory(strID, dadaStrings, dataNum);
                                    DbClass.UpdateCmd(strID, "cmdName", "ok");
                                }
                                else//停止读取,复位结构体成员
                                {
                                    tHistory.IsNeedHistory = false;
                                    tHistory.currentNum = 0;
                                    //跳出for循环
                                    break;
                                }
                            }//end of for
                            
                        }
                        break;


                    #endregion


                    #region 设备升级相关命令
                    //升级
                    case 0x1E:
                        if (tUpdate.IsNeedUpdate == true)
                        {
                            if (datagramBytes[10] == 0x55)
                            {
                                //写入数据库
                                DbClass.UpdateCmd(strID, "data", UtilClass.hex2String[tUpdate.currentNum]);
                                tUpdate.currentNum++;                                
                            }
                            else if (datagramBytes[10] == 0xAA)
                            {
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "fail");
                            }
                            //最多256个包(256K)
                            if (tUpdate.currentNum == 256)
                            {
                                tUpdate.IsNeedUpdate = false;
                                tUpdate.currentNum = 0;
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "ok");
                            }
                        }
                        break;

                    //重启（并升级）
                    case 0x21:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        break;

                    //读取版本号
                    case 0x24:
                        //写入数据库
                        DbClass.UpdateCmd(strID, "data", UtilClass.hex2String[datagramBytes[10]]);
                        break;


                    #endregion


                    #region 开关门时长，本地ip和时间
                    //DS1302时间
                    case 0x02:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]] + UtilClass.hex2String[datagramBytes[13]] +
                                UtilClass.hex2String[datagramBytes[14]]);
                            DbClass.UpdateCmd(strID, "cmdName", "ok");//命令执行成功
                        }
                        break;

                    //local ip
                    case 0x03:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]] + UtilClass.hex2String[datagramBytes[13]]);
                            DbClass.UpdateCmd(strID, "cmdName", "ok");//命令执行成功
                        }
                        break;

                    //开门时长（取值为1-255，不能为0）
                    case 0x07:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                            DbClass.UpdateCmd(strID, "cmdName", "ok");//命令执行成功
                        }
                        break;

                    //关门时长，若为0，表示不检测反馈
                    case 0x08:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;


                    #endregion


                    #region 关系命令                 
                    //读头A关系
                    case 0x09:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;

                    //读头B关系
                    case 0x0A:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;

                    //按键A关系
                    case 0x0B:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;

                    //按键B关系
                    case 0x0C:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    #endregion

                    #region 8个开门方式的配置开关，0-关，1-开,互锁、首卡、多重卡不能同时开启
                    //互锁
                    case 0x0D:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //首卡
                    case 0x0E:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //多重卡
                    case 0x0F:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //超级卡
                    case 0x10:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //超级密码
                    case 0x11:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //胁迫卡
                    case 0x12:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //胁迫码
                    case 0x13:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    //指纹模块
                    case 0x14:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]]);
                        }
                        break;
                    #endregion

                    #region 多种开门方式的卡号配置
                    //首卡
                    case 0x15:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]]);
                        }
                        break;
                    //超级卡
                    case 0x16:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]]);
                        }
                        break;
                    //超级密码
                    case 0x17:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]]);
                        }
                        break;
                    //胁迫卡，3张
                    case 0x18:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]] + UtilClass.hex2String[datagramBytes[13]] +
                                UtilClass.hex2String[datagramBytes[14]] +
                                UtilClass.hex2String[datagramBytes[15]] + UtilClass.hex2String[datagramBytes[16]] +
                                UtilClass.hex2String[datagramBytes[17]] +
                                UtilClass.hex2String[datagramBytes[18]]);
                        }
                        break;
                    //胁迫码
                    case 0x19:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]]);
                        }
                        break;
                    //按键密码
                    case 0x1A:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data",
                                UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                                UtilClass.hex2String[datagramBytes[12]]);
                        }
                        break;
                    //多重卡,10张
                    case 0x1B:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        else if (datagramBytes[7] == 0x00)
                        {
                            byte[] tenCard = new byte[30];
                            Array.Copy(datagramBytes, 10, tenCard, 0, 30);
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data", UtilClass.byteToHexStr(tenCard));
                        }
                        break;



                    #endregion                   

                    #region 普通卡和指纹的配置  
                    case 0x1D://普通卡，第9个字节表示第几包
                        if (tGeneralCardId.IsNeedSet == true)
                        {
                            if (datagramBytes[7] == 0x00)
                            {                                
                                Array.Copy(datagramBytes, 10, generalCardID, tGeneralCardId.currentNum*300, 300);
                                tGeneralCardId.currentNum++;
                            }

                            if (datagramBytes[10] == 0x55)
                            {
                                tGeneralCardId.currentNum++;
                            }
                            else if (datagramBytes[10] == 0xAA)
                            {
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "fail");
                            }
                            //最多5个包(1500字节，500个卡号)
                            if (tGeneralCardId.currentNum == 5)
                            {
                                tGeneralCardId.IsNeedSet = false;
                                tGeneralCardId.currentNum = 0;
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "ok");
                                if (tGeneralCardId.rw == 0)
                                {
                                    DbClass.UpdateCmd(strID, "data", UtilClass.byteToHexStr(generalCardID));
                                }
                            }
                        }
                        break;

                    case 0x1C://指纹号，第9个字节表示第几包
                        if (tFingerId.IsNeedSet == true)
                        {
                            if (datagramBytes[7] == 0x00)
                            {
                                Array.Copy(datagramBytes, 10, fingerID, tFingerId.currentNum * 300, 300);
                                tFingerId.currentNum++;
                            }

                            if (datagramBytes[10] == 0x55)
                            {
                                tFingerId.currentNum++;
                            }
                            else if (datagramBytes[10] == 0xAA)
                            {
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "fail");
                            }
                            //最多5个包(1500字节，500个卡号)
                            if (tFingerId.currentNum == 5)
                            {
                                tFingerId.IsNeedSet = false;
                                tFingerId.currentNum = 0;
                                //写入数据库
                                DbClass.UpdateCmd(strID, "cmdName", "ok");
                                if (tFingerId.rw == 0)
                                {
                                    DbClass.UpdateCmd(strID, "data", UtilClass.byteToHexStr(fingerID));
                                }
                            }
                        }
                        break;


                    #endregion

                    #region 重置参数、远程开门、报警消息、当前刷卡号
                    case 0x06:
                        string door = "-1";
                        DbClass.UpdateSensorInfo(strID, "cardID_now",
                            UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                            UtilClass.hex2String[datagramBytes[12]]);
                        //门号, 高四位表示门号
                        if (datagramBytes[7] == 0x00)
                        {
                            door = "A";
                        }
                        else if (datagramBytes[7] == 0x01)
                        {
                            door = "B";
                        }
                        DbClass.UpdateSensorInfo(strID, "door_now", door);
                        break;

                    case 0x22:
                        DbClass.UpdateSensorInfo(strID, "cardID_now",
                            UtilClass.hex2String[datagramBytes[10]] + UtilClass.hex2String[datagramBytes[11]] +
                            UtilClass.hex2String[datagramBytes[12]]);
                        DbClass.UpdateSensorInfo(strID, "door_now", "22");
                        break;

                    case 0x1F:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        break;

                    //远程开门，低a高b
                    case 0x20:
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        break;

                    #endregion
                    default:
                        break;                        
                }

                if (tHistory.IsNeedHistory == true)
                {
                    SendCmd(SetHisCmd(tHistory.currentNum));
                }
                if (tUpdate.IsNeedUpdate == true)
                {    
                     SendCmd(SetUpdateCmd(tUpdate.currentNum));
                }

                if (tGeneralCardId.IsNeedSet == true)
                {
                    SendCmd(SetGeneralCardID(tGeneralCardId.currentNum, tGeneralCardId.rw));
                }
                if (tFingerId.IsNeedSet == true)
                {
                    SendCmd(SetFingerID(tFingerId.currentNum, tFingerId.rw));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
            }
#if DEBUG
            UtilClass.writeLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "硬件"+strID+"收到数据："+UtilClass.byteToHexStr(datagramBytes));
#endif
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        private void SendCmd(byte[] cmd)
        {
            try
            {
#if DEBUG
                UtilClass.writeLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "硬件" + strID + "发送数据：" + UtilClass.byteToHexStr(cmd));
#endif
                socket.BeginSendTo(cmd, 0, cmd.Length, SocketFlags.None, remote, new AsyncCallback(OnSend), socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
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
                UtilClass.writeLog(ex.ToString());
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

        private byte[] SetUpdateCmd(int bulkCount)
        {
            /***************************************************************************7-读写位*8，9第几包**10-数据位*************************************/
            byte[] Cmd = new byte[1024+13];
            byte[] bytesbulkCount = new byte[2];
            bytesbulkCount = intToBytes(bulkCount);

            Cmd[0] = 0xA5;
            Cmd[1] = 0xA5;
            Cmd[2] = 0x1E;
            Cmd[3] = byteID[0];
            Cmd[4] = byteID[1];
            Cmd[5] = byteID[2];
            Cmd[6] = byteID[3];
            Cmd[7] = 0x01;
            Cmd[8] = bytesbulkCount[0];
            Cmd[9] = bytesbulkCount[1];
            for (int i = 0; i < 1024; i++)
            {
                Cmd[10 + i] = updateData[bulkCount * 1024 + i];
            }

            Cmd[1024 + 10 + 0] = 0xFF;
            Cmd[1024 + 10 + 1] = 0x5A;
            Cmd[1024 + 10 + 2] = 0x5A;
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

        private byte[] SetGeneralCardID(int bulkCount, byte rw)
        {
            //每包300字节，100个卡号
            /***************************************************************************7-读写位*8，9第几包**10-数据位*************************************/
            byte[] Cmd = new byte[300 + 13];
            byte[] bytesbulkCount = new byte[2];
            bytesbulkCount = intToBytes(bulkCount);

            Cmd[0] = 0xA5;
            Cmd[1] = 0xA5;
            Cmd[2] = 0x1D;
            Cmd[3] = byteID[0];
            Cmd[4] = byteID[1];
            Cmd[5] = byteID[2];
            Cmd[6] = byteID[3];
            Cmd[7] = rw;
            Cmd[8] = bytesbulkCount[0];
            Cmd[9] = bytesbulkCount[1];
            for (int i = 0; i < 300; i++)
            {
                Cmd[10 + i] = generalCardID[bulkCount * 300 + i];
            }

            Cmd[300 + 10 + 0] = 0xFF;
            Cmd[300 + 10 + 1] = 0x5A;
            Cmd[300 + 10 + 2] = 0x5A;
            return (Cmd);

        }

        private byte[] SetFingerID(int bulkCount, byte rw)
        {
            //每包300字节，100个卡号
            /***************************************************************************7-读写位*8，9第几包**10-数据位*************************************/
            byte[] Cmd = new byte[300 + 13];
            byte[] bytesbulkCount = new byte[2];
            bytesbulkCount = intToBytes(bulkCount);

            Cmd[0] = 0xA5;
            Cmd[1] = 0xA5;
            Cmd[2] = 0x1C;
            Cmd[3] = byteID[0];
            Cmd[4] = byteID[1];
            Cmd[5] = byteID[2];
            Cmd[6] = byteID[3];
            Cmd[7] = rw;
            Cmd[8] = bytesbulkCount[0];
            Cmd[9] = bytesbulkCount[1];
            for (int i = 0; i < 300; i++)
            {
                Cmd[10 + i] = fingerID[bulkCount * 300 + i];
            }

            Cmd[300 + 10 + 0] = 0xFF;
            Cmd[300 + 10 + 1] = 0x5A;
            Cmd[300 + 10 + 2] = 0x5A;
            return (Cmd);
        }

    }
}
