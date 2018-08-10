using System;
using System.Linq;

namespace SlipperyFisch
{
    public class LogMessage : IMessage
    {
        public LogMessage(string method, string @event, object context)
        {

            Time = DateTime.UtcNow.ToString("o");
            Event = @event;
            Method = method;
            if (context is Exception)
                Exception = context.ToString();
            else
                Details = context;
        }

        public string Time { get; set; }
        public string Event { get; set; }
        public object Details { get; set; }
        public string Exception { get; set; }
        public string Method { get; set; }

        public string AsString()
        {
            var message = string.IsNullOrWhiteSpace(Exception) ? ConvertDetailsToString() : Exception;
            return 
$@"***************************************************************************************************************************************************************
{Time}: {Event}
    {message}
_______________________________________________________________________________________________________________________________________________________________
";
        }
        public string ConvertDetailsToString() {
            if (Details is string)
                return (string)Details;
            else
                return string.Empty;
        }
    }
}
