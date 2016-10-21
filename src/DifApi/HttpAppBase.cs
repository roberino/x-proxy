using LinqInfer.Data.Remoting;
using System;

namespace DifApi
{
    public class HttpAppBase : IDisposable
    {
        protected readonly IOwinApplication _host;

        public HttpAppBase(Uri hostAddress)
        {
            _host = hostAddress.CreateHttpApplication();

            Setup(_host);
        }

        public virtual void Dispose()
        {
            _host.Dispose();
        }

        public void Start()
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }

        protected virtual void Setup(IOwinApplication host)
        {
        }
    }
}