using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

using Topshelf;


namespace TopshelfService
{
    class Program
    {
        /* 
         * Topshelf 框架安装服务常用命令 
         * 安装：TopshelfService.exe install 
         * 启动：TopshelfService.exe start 
         * 停止：TopshelfService.exe stop 
         * 卸载：TopshelfService.exe uninstall 
         */

        static void Main(string[] args)
        {
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4netMySql.config");
            XmlConfigurator.ConfigureAndWatch(logCfg);


            HostFactory.Run(x =>
            {
                //使用 TownCrier 类，配置服务事件  
                x.Service<TownCrier>(s =>
                {
                    //使用自定义的服务  
                    s.ConstructUsing(name => new TownCrier());
                    //服务启动事件  
                    s.WhenStarted(tc => tc.Start());
                    //服务停止后事件  
                    s.WhenStopped(tc => tc.Stop());
                    //服务停止后继续运行时事件  
                    s.WhenContinued(tc => tc.Continued());
                    //服务暂定事件  
                    s.WhenPaused(tc => tc.Paused());
                    //服务卸载事件  
                    s.WhenShutdown(tc => tc.Shutdown());
                });


                #region 服务配置  

                x.RunAsLocalSystem();


                #region 启动类型  
                x.StartAutomatically();//自动运行  
                                       //x.StartManually();//手动运行  
                                       //x.StartAutomaticallyDelayed();//自动延迟运行  
                                       //x.Disabled();//禁用  
                #endregion


                #region 服务信息  
                x.SetDescription("111 服务 111服务的描述");//描述  
                x.SetDisplayName("111 显示名");//显示名称  
                x.SetServiceName("111服务");//服务名称  
                #endregion


                #endregion
            });
        }//end of main
    }




    //服务调用的类，其中的函数对应服务中的事件，如启动事件、暂停事件、恢复事件、继续运行事件等  
    public class TownCrier
    {
        readonly Timer _timer;

        public static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TownCrier()
        {
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) =>
            {
                string str = "It is {0} and all is well" +　DateTime.Now.ToString();
                Console.WriteLine(str);
                log.Info(str);
            };
        }
        public void Start() { _timer.Start(); Msg("已启动"); }
        public void Stop() { _timer.Stop(); Msg("已停止"); }
        public void Continued() { Msg("继续运行"); }
        public void Paused() { Msg("已暂停"); }
        public void Shutdown() { Msg("已卸载"); }


        public void Msg(string msg)
        {
            /*ServiceEvents.Msg(msg);
            if (msg != null)
                msg.All(x => { Console.WriteLine(x); return true; });*/
            Console.WriteLine(msg);
        }
    }

}



