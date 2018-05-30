using System;
using System.IO;
using System.Linq;
using Castle.DynamicProxy;
using NLog;

namespace AOPLogger
{
    public class DPLogger : IInterceptor
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        public static Serializer Serializer = new Serializer();

        public void Intercept(IInvocation invocation)
        {
            Logger.Info("TimeStamp: {0} Calling method {1} with parameters {2}... ",
                DateTime.Now,
                Serializer.Serialize(invocation.Method),
                Serializer.Serialize(invocation.Arguments)
                );

            invocation.Proceed();

            Logger.Info("Done: result was {0}.", invocation.ReturnValue);
        }
    }
}
