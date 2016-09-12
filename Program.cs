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
            var port = System.Configuration.ConfigurationManager.AppSettings["ListenPort"];
            if ( string.IsNullOrWhiteSpace ( port ) )
                port = "8888";

            var serveMe = new SimpleHTTPServer("", int.Parse( port));
            System.Console.WriteLine ( "Now listeing on port: " + port );
            System.Console.ReadKey ();
            serveMe.Stop ();
            
        }
    }
}
