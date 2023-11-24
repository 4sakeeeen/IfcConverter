using Serilog;
using Serilog.Core;
using System.Windows;

namespace IfcConverter.Client
{
    public partial class App : Application
    {
        public static readonly Logger Logger
            = new LoggerConfiguration().WriteTo.File("logs\\common.log", rollingInterval: RollingInterval.Day).CreateLogger();
    }
}
