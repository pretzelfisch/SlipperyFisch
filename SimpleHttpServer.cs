using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace SlipperyFisch
{

    public class SimpleHTTPServer
    {
        private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };
        private readonly ILogging _logger;
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".svg", "image/svg+xml"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;
        private bool _isSPA;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        public SimpleHTTPServer(ServerConfiguration configuration )
        {
            this._logger = configuration.Logger;
            this._isSPA = configuration.isSPA;
            if (!configuration.Port.HasValue)
            {
                //get an empty port
                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                configuration.Port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
            }
            this.Initialize(configuration.VirtualDirectory, configuration.Port.Value);
        }

        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    string rawRequest = string.Empty;

                    System.Collections.Specialized.NameValueCollection headers = context.Request.Headers;
                    // Get each header and display each value.
                    foreach (string key in headers.AllKeys)
                    {
                        string[] values = headers.GetValues(key);
                        if (values.Length > 0)
                        {
                            foreach (string value in values)
                            {
                                rawRequest += System.Environment.NewLine + string.Format("{0}:{1}", key, value);
                            }
                        }
                    }

                    using (System.IO.Stream body = context.Request.InputStream) // here we have data
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(body, context.Request.ContentEncoding))
                        {
                            rawRequest += System.Environment.NewLine + reader.ReadToEnd();
                        }
                    }
                    _logger.Info(new LogMessage(nameof(Listen), $"{context.Request.HttpMethod}: {context.Request.Url.ToString()}", rawRequest));
                    Process(context);
                }
                catch (Exception ex)
                {
                    _logger.Error(new LogMessage(nameof(Listen), "UnHandledError",  ex));
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename)||( this._isSPA && filename.Contains(".") == false) )
            {
                foreach (string indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }

            filename = Path.Combine(_rootDirectory, filename);

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    _logger.Error(new LogMessage(nameof(Process), "UnHandledError", ex));
                    WriteResponse(context.Response, (int)HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                WriteResponse(context.Response, (int)HttpStatusCode.NotFound);
            }
            context.Response.OutputStream.Close();
        }

        private void Initialize(string path, int port)
        {
            this._rootDirectory = path;
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

        void WriteResponse(HttpListenerResponse response, int statusCode)
        {
            var outputStream = response.OutputStream;

            response.ContentType = "text/html";
            response.StatusCode = statusCode;
            response.AddHeader("Date", DateTime.Now.ToString("r"));

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            var resourceName = $"SlipperyFisch.content.page{statusCode}.html";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                response.ContentLength64 = stream.Length;
                int byteData = 0;
                while (true)
                {
                    byteData = stream.ReadByte();
                    if (byteData >= 0)
                    {
                        outputStream.WriteByte((byte)byteData);
                    }
                    else
                    {
                        outputStream.Flush();
                        break;
                    }
                }
            }


        }


    }
}
