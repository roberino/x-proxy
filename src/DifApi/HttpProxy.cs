using LinqInfer.Data.Remoting;
using System;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace DifApi
{
    class HttpProxy : HttpAppBase
    {
        private readonly Uri[] _targets;
        private readonly RequestStore _store;

        public HttpProxy(Uri hostAddress, Uri[] targets, RequestStore storage) : base(hostAddress)
        {
            _targets = targets;
            _store = storage;
        }

        public RequestStore Storage { get { return _store; } }

        protected override void Setup(IOwinApplication host)
        {
            host.AddComponent(async c =>
            {
                var target = _targets[0];

                using (var client = new HttpClient())
                {
                    var fwdUri = new Uri(c.RequestUri.Scheme + Uri.SchemeDelimiter + target.Host + ":" + target.Port + c.RequestUri.PathAndQuery);

                    if (fwdUri.Scheme != Uri.UriSchemeHttp) return;

                    var request = new HttpRequestMessage()
                    {
                        RequestUri = fwdUri
                    };

                    Console.WriteLine("Fwd request headers:");

                    foreach (var header in c.Request.Header.Headers)
                    {
                        if (header.Key.StartsWith("Content") || header.Key.StartsWith("Accept") || header.Key == "Cookie" || header.Key == "User-Agent")
                        {
                            request.Headers.Add(header.Key, header.Value);

                            Console.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                        }
                    }

                    request.Headers.Add("Host", target.Host);
                    request.Method = new HttpMethod(c.Request.Header.HttpVerb);

                    if (c.Request.Header.ContentLength > 0)
                    {
                        request.Content = new ForwardedRequestContent(c);
                    }

                    var res = await client.SendAsync(request);

                    Console.WriteLine("Fwd response headers:");

                    foreach (var header in res.Headers)
                    {
                        Console.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                        c.Response.Header.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in res.Content.Headers)
                    {
                        Console.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                        c.Response.Header.Headers[header.Key] = header.Value.ToArray();
                    }

                    if (!c.Response.Header.Headers.ContainsKey("Content-Type"))
                    {
                        c.Response.Header.Headers["Content-Type"] = new[] { "application/json; charset=utf-8" };
                    }
                    c.Response.Header.StatusCode = (int)res.StatusCode;

                    await res.Content.CopyToAsync(c.Response.Content);

                    await _store.StoreRequest(c);
                }
            });
        }

        private class ForwardedRequestContent : HttpContent
        {
            private readonly IOwinContext _context;

            public ForwardedRequestContent(IOwinContext context)
            {
                _context = context;
            }

            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return Task.FromResult(_context.Request.Content);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _context.Request.Content.Length;
                return true;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return _context.Request.Content.CopyToAsync(stream);
            }
        }
    }
}