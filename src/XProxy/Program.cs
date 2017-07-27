using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XProxy.Core;

namespace XProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var proxyUri = new Uri(args[0]);
            var targetUris = args.Skip(1).Where(a => Regex.IsMatch(a, @"^https?\:\/\/")).Select(a => new Uri(a)).ToArray();
            var controlUri = new Uri(proxyUri.Scheme + "://" + proxyUri.Host + ":9373");
            var uiUri = new Uri(proxyUri.Scheme + "://" + proxyUri.Host + ":8080");

            var baseDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "data"));

            using (var startup = new Startup(baseDir, proxyUri, targetUris, controlUri, uiUri))
            {
                startup.Start();

                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                    {
                        Console.WriteLine();
                        Console.Write(startup);

                        continue;
                    }
                    break;
                }

                startup.Stop();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: XProxy [proxy-url] [target-url1] [target-url2]...");
        }
    }
}