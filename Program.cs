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
            var SPA = System.Configuration.ConfigurationManager.AppSettings["SPAmode"];
            if ( string.IsNullOrWhiteSpace ( port ) )
                port = "8888";
            var SPAmode = false;
            bool.TryParse(SPA, out SPAmode);

            var serveMe = new SimpleHTTPServer("", int.Parse( port), SPAmode);
            System.Console.WriteLine ( "Now listening on port: " + port );
            System.Console.ReadKey ();
            serveMe.Stop ();
            
        }
    }
}
