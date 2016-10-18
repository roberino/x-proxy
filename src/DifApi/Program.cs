using System;

namespace DifApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxyUri = new Uri(args[0]);
            var targetUri = new Uri(args[1]);
            var controlUri = new Uri(proxyUri.Scheme + Uri.SchemeDelimiter + proxyUri.Host + ":9373");

            var store = new RequestStore(new System.IO.DirectoryInfo(Environment.CurrentDirectory));

            using (var proxy = new HttpProxy(proxyUri, new[] { targetUri }, store))
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