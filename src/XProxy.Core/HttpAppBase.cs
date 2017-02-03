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
            //_host = hostAddress.CreateHttpApplication();
        }

        internal IOwinApplication ApplicationHost { get { return _host; } }

        public void AllowOrigin(Uri origin)
        {
            _host.AddComponent(c =>
            {
                c.Response.Header.Headers["Access-Control-Allow-Credentials"] = new[] { "true" };
                c.Response.Header.Headers["Access-Control-Allow-Origin"] = new[] { origin.Scheme + Uri.SchemeDelimiter + origin.Host + ':' + origin.Port };
                c.Response.Header.Headers["Access-Control-Allow-Methods"] = new[] { "GET, POST, PUT, DELETE, OPTIONS" };
                c.Response.Header.Headers["Access-Control-Allow-Headers"] = new[] { "Content-Type,X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5,  Date, X-Api-Version, X-File-Name" };

                if (c.Request.Header.HttpVerb == "OPTIONS")
                {
                    c.Response.Header.StatusCode = 200;
                }

                return Task.FromResult(true);
            }, OwinPipelineStage.Authenticate);
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