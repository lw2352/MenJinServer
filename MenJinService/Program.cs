using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

using Topshelf;


namespace MenJinService
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
                x.Service<MainUdpClass>(s =>
                {
                    //使用自定义的服务  
                    s.ConstructUsing(name => new MainUdpClass());
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
                x.SetDescription("威派门禁服务");//描述  
                x.SetDisplayName("威派门禁服务");//显示名称  
                x.SetServiceName("威派门禁服务");//服务名称  
                #endregion


                #endregion
            });
        }//end of main
    }


}



