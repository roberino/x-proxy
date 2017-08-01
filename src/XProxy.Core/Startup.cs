using XProxy.Core.Analysers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XProxy.Core.Jobs;
using XProxy.Core.Events;
using XProxy.Core.Analysers.Faults;

namespace XProxy.Core
{
    public class Startup : IDisposable
    {
        private readonly List<HttpAppBase> _apps;
        private readonly JobRunner _jobRunner;

        public Startup(DirectoryInfo baseDataDir, Uri proxyUri, Uri[] targetUris, Uri controlUri, Uri uiUri = null) : this(SessionStore.CreateSessionStore(baseDataDir), proxyUri, targetUris, controlUri, uiUri)
        {
        }

        public Startup(SessionStore sessionStore, Uri proxyUri, Uri[] targetUris, Uri controlUri, Uri uiUri = null)
        {
            var baseStore = sessionStore;
            var logger = new HttpLogger(baseStore.BaseStorageDirectory);
            var store = new RequestStore(baseStore.BaseStorageDirectory, logger);

            _jobRunner = new JobRunner(new ExecutionContext()
            {
                SessionStore = baseStore,
                EventDispatcher = new FileSystemEventDispatcher(baseStore.BaseStorageDirectory),
                HttpLogs = logger,
                RequestStore = store,
                ServiceEndpoint = controlUri,
                Logger = Console.Out
            });

            _apps = new List<HttpAppBase>();

            var proxy = new HttpProxy(proxyUri, targetUris);

            var faultAnalysis = new FaultAnalyser(sessionStore);

            _jobRunner.Register(new DiffEngine());

            {
                proxy.AnalyserEngine.Register(store);
                // proxy.AnalyserEngine.Register(faultAnalysis);
                proxy.AnalyserEngine.Register(logger);
                proxy.AnalyserEngine.Register(new TextIndexer(baseStore.BaseStorageDirectory, logger));
                proxy.AnalyserEngine.Register(new HttpComparer(store));

                //proxy.AnalyserEngine.Register(new RequestFeatureMap(baseDir, logger));

                proxy.ApplicationHost.StatusChanged += (s, v) =>
                {
                    Console.WriteLine("Proxy status = {0}", v.Value);
                };

                var control = new HttpController(controlUri, proxy);

                control.RegisterHttpService((IHasHttpInterface)_jobRunner.Context.EventDispatcher);

                Console.WriteLine("Binding to {0}", proxyUri);
                Console.WriteLine("Control via {0}", controlUri);

                if (uiUri != null)
                {
                    var app = new WebPortal(uiUri);

                    control.AllowOrigin(uiUri);

                    _apps.Add(app);

                    Console.WriteLine("Admin portal via {0}", uiUri);
                }

                _apps.Add(proxy);
                _apps.Add(control);
            }
        }

        public void Start()
        {
            foreach (var app in _apps) app.Start();

            _jobRunner.Start();
        }

        public void Stop()
        {
            foreach (var app in _apps) app.Stop();

            _jobRunner.Stop();
        }

        public void Dispose()
        {
            foreach (var app in _apps) app.Dispose();

            _jobRunner.Dispose();
        }

        public override string ToString()
        {
            var status = new StringBuilder();

            foreach (var app in _apps)
            {
                status.AppendFormat("{0} {1}\n", app.ApplicationHost.BaseEndpoint, app.ApplicationHost.Status);
            }

            return status.ToString();
        }
    }
}