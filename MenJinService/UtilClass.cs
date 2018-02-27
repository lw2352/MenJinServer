using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenJinService
{
    //byte与string的配对结构体
    struct Hex2string
    {
        public byte hex;
        public string str;
    };

    /// <summary>
    /// 工具函数类
    /// </summary>
    class UtilClass
    {

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Hex2string[] hex2String = new Hex2string[256];

        /// <summary>
        /// debug模式下会打印到控制台，release输出到数据库
        /// </summary>
        /// <param name="msg"></param>
        public static void writeLog(string msg)
        {
            Console.WriteLine(msg + "\r\n");

            log.Info(msg);
        }

        /// <summary>
        /// 初始化工具类，生成hex与string的对应关系
        /// </summary>
        public static void initUtil()
        {
            for (int i = 0; i < hex2String.Length; i++)
            {
                hex2String[i].hex = (byte)i;
                hex2String[i].str = i.ToString("X2");
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
                for (long i = 0; i < bytes.Length; i++)
                {
                    returnStr += hex2String[bytes[i]].str;
                }
            }
            return returnStr;
        }


    }
}
