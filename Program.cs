using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipperyFisch
{
    class Program
    {
       
        static void Main ( string[] args )
        {
            var config = GetConfigurationSettins();

            var serveMe = new SimpleHTTPServer(config);
            System.Console.WriteLine ( "Now listening on port: " + config.Port.ToString() );
            System.Console.ReadKey ();
            serveMe.Stop ();
        }
        static ServerConfiguration GetConfigurationSettins() {

            var virtualDirectoryPath = System.Configuration.ConfigurationManager.AppSettings["VirtualDirectory"];
            var port = System.Configuration.ConfigurationManager.AppSettings["ListenPort"];
            var SPA = System.Configuration.ConfigurationManager.AppSettings["SPAmode"];
            //TODO: read logging directory

            return new ServerConfiguration(virtualDirectoryPath, port, SPA, new SimpleLogger( System.Environment.CurrentDirectory));
        }

    }
}
