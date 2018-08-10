using System;
using System.Linq;

namespace SlipperyFisch
{
    public class ServerConfiguration
    {
        const bool DOES_NOT_EXIST = false;
        public ServerConfiguration(string virtualDirectory, string port, string SPA, ILogging logger)
        {
            Logger = logger;

            if (string.IsNullOrWhiteSpace(port))
                port = "8888";
            if (string.IsNullOrWhiteSpace(virtualDirectory))
            {
                virtualDirectory = "";
            }
            else if (System.IO.Directory.Exists(virtualDirectory) == DOES_NOT_EXIST)
            {
                virtualDirectory = "";
            }

            var SPAmode = false;
            bool.TryParse(SPA, out SPAmode);


            VirtualDirectory = virtualDirectory;
            Port = int.Parse(port);
            this.isSPA = SPAmode;
        }
        public string VirtualDirectory { get; set; }
        public int? Port { get; set; }
        public bool isSPA { get; set; }
        public ILogging Logger { get; set; }
    }
}
