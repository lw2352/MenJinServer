﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenJinService
{
    /// <summary>
    /// 命令构造类
    /// </summary>
    class CmdClass
    {
        private static Hashtable htCmdNum = new Hashtable(); //str--hex

        /// <summary>
        /// 命令初始化
        /// </summary>
        public static void cmdInit()
        {
            //读写操作
            htCmdNum.Add("read", 0x00);
            htCmdNum.Add("write", 0x01);
            //命令号
            htCmdNum.Add("search", 0x00);
            htCmdNum.Add("time", 0x02);
            htCmdNum.Add("localIP", 0x03);
            htCmdNum.Add("reboot", 0x21);
            htCmdNum.Add("version", 0x24);

            htCmdNum.Add("openTime", 0x07);
            htCmdNum.Add("waitTime", 0x08);

            htCmdNum.Add("relationReaderA", 0x09);
            htCmdNum.Add("relationReaderB", 0x0A);
            htCmdNum.Add("relationButtonA", 0x0B);
            htCmdNum.Add("relationButtonB", 0x0C);

            htCmdNum.Add("reset", 0x1F);
            htCmdNum.Add("remoteOpen", 0x20);
            //8种开门功能是否开启，0/1，待测
            htCmdNum.Add("interlock", 0x0D);
            htCmdNum.Add("firstCard", 0x0E);
            htCmdNum.Add("multipleCard", 0x0F);
            htCmdNum.Add("superCard", 0x10);
            htCmdNum.Add("superPassword", 0x11);
            htCmdNum.Add("threatCard", 0x12);
            htCmdNum.Add("threatPassword", 0x13);
            htCmdNum.Add("finger", 0x14);

            htCmdNum.Add("firstCardID", 0x15);
            htCmdNum.Add("superCardID", 0x16);
            htCmdNum.Add("superPasswordID", 0x17);
            htCmdNum.Add("threatCardID", 0x18);//最多3张
            htCmdNum.Add("threatPasswordID", 0x19);
            htCmdNum.Add("keyBoardID", 0x1A);
            htCmdNum.Add("multipleCardID", 0x1B);
        }

        public static byte[] makeCommand(string cmdFlag, string rw, string data, byte[] mcuID)
        {
            try
            {
                if (cmdFlag == null || rw == null || data == null || mcuID == null || cmdFlag == "-1" || rw == "-1" || data == "-1")
                {
                    return null;
                }
                byte len = (byte)(data.Length / 2);
                byte[] buf = new byte[len + 13];
                byte[] byteData = UtilClass.hexStrToByte(data);
                int i;
                byte crcRet;

                buf[0] = 0xA5;
                buf[1] = 0xA5;
                if (!htCmdNum.ContainsKey(cmdFlag) || !htCmdNum.ContainsKey(rw))
                {
                    return null;
                }

                buf[2] = Convert.ToByte(htCmdNum[cmdFlag]);
                buf[3] = mcuID[0];
                buf[4] = mcuID[1];
                buf[5] = mcuID[2];
                buf[6] = mcuID[3];
                buf[7] = Convert.ToByte(htCmdNum[rw]);
                buf[8] = (byte)(len >> 8);
                buf[9] = (byte)(len & 0xFF);

                for (i = 0; i < len; i++)
                {
                    buf[10 + i] = byteData[i];
                }

                crcRet = Get_Crc8(buf, 10 + len);

                buf[10 + len + 0] = crcRet;
                buf[10 + len + 1] = 0x5A;
                buf[10 + len + 2] = 0x5A;

                return buf;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
                return null;
            }
            
        }

        private static byte Get_Crc8(byte[] data, int len)
        {
            return 0xFF;
        }
    }
}
