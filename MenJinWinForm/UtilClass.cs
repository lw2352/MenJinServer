using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenJinWinForm
{
    /// <summary>
    /// 工具函数类
    /// </summary>
    class UtilClass
    {

       // private static readonly log4net.ILog log =
            //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string[] hex2String = new string[256];
        private static Hashtable htStrToHex = new Hashtable(); //str--hex

        /// <summary>
        /// debug模式下会打印到控制台，release输出到数据库
        /// </summary>
        /// <param name="msg"></param>
        public static void writeLog(string msg)
        {
            Console.WriteLine(msg + "\r\n");

            //log.Info(msg);
        }

        /// <summary>
        /// 初始化工具类，生成hex与string的对应关系
        /// </summary>
        public static void utilInit()
        {
            for (int i = 0; i < hex2String.Length; i++)
            {
                hex2String[i] = i.ToString("X2");
                htStrToHex.Add(hex2String[i], (byte)i);
            }
        }

        /// <summary>
        /// 字节数组转16进制字符串 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += hex2String[bytes[i]];
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 例如："7985"->[0x79,0x85]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] hexStrToByte(string str)
        {
            byte[] bytes = new byte[str.Length/2];
            string a;
            for (int i = 0,j=0; i < str.Length; i++,i++,j++)
            {
                a= str.Substring(i, 2);
                bytes[j] = (byte)htStrToHex[a];
            }

            return bytes;

        }


    }
}
