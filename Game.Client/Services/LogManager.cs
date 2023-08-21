using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client
{
    public class LogManager
    {
        public LogManager()
        {
            Log.Logger = new LoggerConfiguration()
           .WriteTo.File(GetLogFilePath(), rollingInterval: RollingInterval.Day)
           .CreateLogger();
        }

        public void LogInfo(string message, object data = null)
        {
            if (data != null)
            {
                var dataAsJson = JsonConvert.SerializeObject(data);
                Log.Information($"{message} : {dataAsJson}");
            }
            else
            {
                Log.Information(message);
            }
        }

        public void LogError(string message, Exception ex)
        {
            Log.Error(message, ex);
        }

        private string GetLogFilePath()
        {
            var directory = new DirectoryInfo(
                 Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return Path.Combine(directory.FullName, "Logs", "GameClient-Logs", "Log-.log");
        }
    }
}
