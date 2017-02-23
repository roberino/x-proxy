using LinqInfer.Data.Remoting;
using LinqInfer.Owin;
using System;
using System.Threading.Tasks;

namespace XProxy.Core
{
    public class HttpAppBase : IDisposable
    {
        protected readonly IOwinApplication _host;

        bool _setup;

        public HttpAppBase(Uri hostAddress, bool bufferResponse = false)
        {
            _host = hostAddress.CreateOwinApplication(bufferResponse);
        }

        internal IOwinApplication ApplicationHost { get { return _host; } }

        public void AllowOrigin(Uri origin)
        {
            _host.AllowOrigin(origin);
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