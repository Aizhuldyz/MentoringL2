using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOPLogger;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Castle.DynamicProxy;

namespace ScannedFileProcessor
{
    public class AutofacConfig
    {
        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ScannedFileProcessingService>()
                .As<IProcessScannedFiles>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(DPLogger));
            builder.Register(c => new DPLogger());
            return builder.Build();
        }
    }
}