using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOPLogger;
using Autofac;
using NLog;
using NLog.Config;
using NLog.Targets;
using Topshelf;

namespace ScannedFileProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ConsoleTarget() { Name = "logconsole" };

            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, logconsole));
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Error, logconsole));
            LogManager.Configuration = config;
            var currentDir = ConfigurationManager.AppSettings["LogFileDir"];
            var conf = new LoggingConfiguration();
            var fileTarget = new FileTarget
            {
                Name = "Default",
                FileName = Path.Combine(currentDir, "log.txt"),
                Layout = "${date} ${message} ${onexception:inner=${exception:format=toString}}"
            };
            
            conf.AddTarget(fileTarget);
            conf.AddRuleForAllLevels(fileTarget);

            var logFactory = new LogFactory(conf);
            var container = AutofacConfig.GetContainer();
            var scannerService = container.Resolve<IProcessScannedFiles>();
            HostFactory.Run(
                hostConf => hostConf.Service<IProcessScannedFiles>(
                    s => {
                        s.ConstructUsing(() => scannerService);
                        s.WhenStarted(serv => serv.Start());
                        s.WhenStopped(serv => serv.Stop());
                    }
                ).UseNLog(logFactory));
        }
    }
}
