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

            using (var startup = new Startup(baseDir, proxyUri, targetUris, controlUri, uiUri))
            {
                startup.Start();

                Console.ReadKey();

                startup.Stop();
            }
        }
    }
}