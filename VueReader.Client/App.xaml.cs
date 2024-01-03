using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace IfcConverter.Client
{
    public partial class App : Application
    {
        public App()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "C:", "logs", "common.log"), rollingInterval: RollingInterval.Day).CreateLogger();
        }
    }
}
