using System;
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

            var store = new RequestStore(new System.IO.DirectoryInfo(Environment.CurrentDirectory));

            using (var proxy = new HttpProxy(proxyUri, targetUris, store))
            using (var control = new HttpController(controlUri, proxy))
            {
                Console.WriteLine("Binding to {0}", proxyUri);
                Console.WriteLine("Control via {0}", controlUri);

                proxy.Start();
                control.Start();

                Console.ReadKey();

                proxy.Stop();
                control.Stop();
            }
        }
    }
}