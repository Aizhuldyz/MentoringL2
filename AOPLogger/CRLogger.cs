using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using PostSharp.Aspects;

namespace AOPLogger
{
    [Serializable]
    public class CRLoggerAttribute : OnMethodBoundaryAspect
    {       
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        public static Serializer Serializer = new Serializer();
        public override void OnEntry(MethodExecutionArgs args)
        {
            Logger.Info($"On Entry timestamp: {DateTime.Now} ");
            Logger.Info($"{Serializer.Serialize(args.Method)} ");
            Logger.Info($"{Serializer.Serialize(args.Arguments)}.");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            Logger.Info($"On Success: {args.Method.Name} ");
            Logger.Info($"{Serializer.Serialize(args.ReturnValue ?? "[null]")}.");

            base.OnSuccess(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            Logger.Info($"On Exception: {args.Method.Name} ");
            Logger.Info($"{Serializer.Serialize(args.Exception)}.");

            base.OnException(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Logger.Info($"On Exit: {args.Method.Name} .");
            base.OnExit(args);
        }

    }
}
