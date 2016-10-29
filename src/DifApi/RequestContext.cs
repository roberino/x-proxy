using LinqInfer.Data.Remoting;
using System;
using System.IO;

namespace DifApi
{
    public class RequestContext : IDisposable
    {
        private RequestContext _parent;

        public RequestContext(Uri originUrl, IOwinContext context, Stream requestBlob)
        {
            Id = Guid.NewGuid();
            OwinContext = context;
            RequestBlob = requestBlob;
            OriginUrl = originUrl;
        }

        public RequestContext Chain(Stream blob)
        {
            return new RequestContext(OriginUrl, OwinContext, blob)
            {
                Elapsed = Elapsed,
                Id = Id,
                _parent = this
            };
        }

        public Guid Id { get; internal set; }
        public TimeSpan Elapsed { get; internal set; }        
        public IOwinContext OwinContext { get; private set; }
        public Stream RequestBlob { get; private set; }
        public Uri OriginUrl { get; private set; }

        public void Dispose()
        {
            RequestBlob.Dispose();

            if (_parent != null)
                _parent.Dispose();
        }
    }
}