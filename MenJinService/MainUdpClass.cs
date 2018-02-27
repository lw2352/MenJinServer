using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenJinService
{
    /// <summary>
    /// udp服务，存放主要逻辑。并且被topshelf调用
    /// </summary>
    class MainUdpClass
    {
        public static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string ConfigIp = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];

        public void Start() { consoleMsg("已启动"); }
        public void Stop() { consoleMsg("已停止"); }
        public void Continued() { consoleMsg("继续运行"); }
        public void Paused() { consoleMsg("已暂停"); }
        public void Shutdown() { consoleMsg("已卸载"); }

        public void consoleMsg(string msg)
        {
            Console.WriteLine(msg+"\r\n");
        }

        public void writeLog(string msg)
        {
            log.Info(msg);
        }


    }
}
