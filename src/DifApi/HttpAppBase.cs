using LinqInfer.Data.Remoting;
using System;

namespace DifApi
{
    public class HttpAppBase : IDisposable
    {
        protected readonly IOwinApplication _host;

        bool _setup;

        public HttpAppBase(Uri hostAddress)
        {
            _host = hostAddress.CreateHttpApplication();
        }

        public virtual void Dispose()
        {
            _host.Dispose();
        }

        public void Start()
        {
            if (!_setup) Setup(_host);
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }

        protected virtual void Setup(IOwinApplication host)
        {
            _setup = true;
        }
    }
}