using LinqInfer.Data.Remoting;
using System;
using System.IO;

namespace XProxy.Core.Models
{
    public class RequestContext : IDisposable
    {
        private RequestContext _parent;

        internal RequestContext(Guid id, Uri originUrl, IOwinContext context, Stream requestBlob)
        {
            Id = id;
            OwinContext = context;
            RequestBlob = requestBlob;
            OriginUrl = originUrl;
        }

        internal RequestContext Chain(Stream blob)
        {
            return new RequestContext(Id, OriginUrl, OwinContext, blob)
            {
                Elapsed = Elapsed,
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