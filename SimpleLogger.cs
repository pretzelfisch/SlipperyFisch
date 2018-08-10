using System;
using System.Linq;

namespace SlipperyFisch
{
    public class SimpleLogger : ILogging
    {

        readonly string loggingPath;
        public string SystemName { get; set; }
        public SimpleLogger(string loggingPath)
        {
            this.loggingPath = loggingPath;
            SystemName = "SlipperyFisch";
        }

        private void CoreLog(IMessage message,  [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {

            
            try
            {
                if (callerName == "Error" || callerName == "Fatal")
                {
                    System.IO.File.AppendAllText(System.IO.Path.Combine(loggingPath, "error.txt"), message.AsString());
                }
                else {
                    System.IO.File.AppendAllText(System.IO.Path.Combine(loggingPath, "log.txt"), message.AsString());
                }
            }
            catch (System.Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(loggingPath, SystemName + ".txt"), DateTime.UtcNow.ToString("o") + System.Environment.NewLine + ex.ToString() );
            }
        }

        public void Debug(IMessage message)
        {
            CoreLog(message);
        }

        public void Error(IMessage message)
        {
            CoreLog(message);
        }

        public void Fatal(IMessage message)
        {
            CoreLog(message);
        }

        public void Info(IMessage message)
        {
            CoreLog(message);
        }

        public void Warn(IMessage message)
        {
            CoreLog(message);
        }
    }
}
