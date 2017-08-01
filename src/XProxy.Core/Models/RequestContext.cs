using LinqInfer.Data.Remoting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Core.Models
{
    public class RequestContext : IDisposable
    {
        private RequestContext _parent;

        internal RequestContext(Guid id, Uri originUrl, IOwinContext context, Stream requestBlob)
        {
            Id = id;
            OwinContext = context;
            OriginUrl = originUrl;

            if (requestBlob is MemoryStream)
            {
                RequestBlob = ((MemoryStream)requestBlob).ToArray();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    requestBlob.CopyTo(ms);
                    RequestBlob = ms.ToArray();
                }
            }
        }

        internal RequestContext Chain(RequestContext context)
        {
            if (!ReferenceEquals(this, context))
            {
                context._parent = this;
            }

            return context;
        }

        public Guid Id { get; internal set; }
        public TimeSpan Elapsed { get; internal set; }        
        public IOwinContext OwinContext { get; private set; }
        public byte[] RequestBlob { get; private set; }
        public MemoryStream GetRequestStream()
        {
            return new MemoryStream(RequestBlob);
        }

        public Task CopyToAsync(Stream output)
        {
            return output.WriteAsync(RequestBlob, 0, RequestBlob.Length);
        }

        public Uri OriginUrl { get; private set; }

        /// <summary>
        /// Creates a reader which is advanced past the header to the position where the content starts
        /// </summary>
        /// <param name="encoding">If null, the response header encoding is used</param>
        /// <returns></returns>
        public TextReader CreateContentReader(Encoding encoding = null)
        {
            var stream = GetRequestStream();
            var reader = new StreamReader(stream, encoding ?? OwinContext.Response.Header.TextEncoding, true, 1024, true);
            int i = 0;

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null) return reader;

                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                }

                if (i > 1) // expecting 2 request + 2 response header lines
                {
                    return reader;
                }
            }
        }

        public void Dispose()
        {
            RequestBlob = null;

            if (_parent != null)
                _parent.Dispose();
        }
    }
}