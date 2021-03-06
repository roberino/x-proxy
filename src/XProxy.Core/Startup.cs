﻿using XProxy.Core.Analysers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XProxy.Core
{
    public class Startup : IDisposable
    {
        private readonly List<HttpAppBase> apps;

        public Startup(DirectoryInfo baseDataDir, Uri proxyUri, Uri[] targetUris, Uri controlUri, Uri uiUri = null)
        {
            var baseDir = baseDataDir;
            var logger = new HttpLogger(baseDir);

            apps = new List<HttpAppBase>();

            var proxy = new HttpProxy(proxyUri, targetUris);

            {
                proxy.AnalyserEngine.Register(new RequestStore(baseDir, logger));
                proxy.AnalyserEngine.Register(logger);
                proxy.AnalyserEngine.Register(new TextIndexer(baseDir, logger));
                //proxy.AnalyserEngine.Register(new RequestFeatureMap(baseDir, logger));

                proxy.ApplicationHost.StatusChanged += (s, v) =>
                {
                    Console.WriteLine("Proxy status = {0}", v.Value);
                };

                var control = new HttpController(controlUri, proxy);

                Console.WriteLine("Binding to {0}", proxyUri);
                Console.WriteLine("Control via {0}", controlUri);

                if (uiUri != null)
                {
                    var app = new WebPortal(uiUri);

                    control.AllowOrigin(uiUri);

                    apps.Add(app);

                    Console.WriteLine("Admin portal via {0}", uiUri);
                }

                apps.Add(proxy);
                apps.Add(control);
            }
        }

        public void Start()
        {
            foreach (var app in apps) app.Start();
        }

        public void Stop()
        {
            foreach (var app in apps) app.Stop();
        }

        public void Dispose()
        {
            foreach (var app in apps) app.Dispose();
        }

        public override string ToString()
        {
            var status = new StringBuilder();

            foreach (var app in apps)
            {
                status.AppendFormat("{0} {1}\n", app.ApplicationHost.BaseEndpoint, app.ApplicationHost.Status);
            }

            return status.ToString();
        }
    }
}