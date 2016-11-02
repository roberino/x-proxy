using DifApi.Analysers;
using System;
using System.IO;
using System.Linq;

namespace DifApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxyUri = new Uri(args[0]);
            var targetUris = args.Skip(1).Where(a => a.StartsWith("http://")).Select(a => new Uri(a)).ToArray();
            var controlUri = new Uri(proxyUri.Scheme + Uri.SchemeDelimiter + proxyUri.Host + ":9373");
            var uiUri = new Uri(proxyUri.Scheme + Uri.SchemeDelimiter + proxyUri.Host + ":8080");

            var baseDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "data"));
            var logger = new HttpLogger(baseDir);

            using (var proxy = new HttpProxy(proxyUri, targetUris))
            {
                proxy.AnalyserEngine.Register(new RequestStore(baseDir, logger));
                proxy.AnalyserEngine.Register(logger);
                proxy.AnalyserEngine.Register(new TextIndexer(baseDir, logger));

                using (var app = new WebApp(uiUri))
                using (var control = new HttpController(controlUri, proxy))
                {
                    control.AllowOrigin(uiUri);

                    Console.WriteLine("Binding to {0}", proxyUri);
                    Console.WriteLine("Control via {0}", controlUri);

                    proxy.Start();
                    control.Start();
                    app.Start();

                    Console.ReadKey();

                    proxy.Stop();
                    control.Stop();
                    app.Stop();
                }
            }
        }
    }
}