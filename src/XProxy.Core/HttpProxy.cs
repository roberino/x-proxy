using LinqInfer.Data.Remoting;
using System;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace XProxy.Core
{
    public class HttpProxy : HttpAppBase
    {
        private readonly Uri[] _targets;
        private readonly RequestAnalysisEngine _analysers;

        public HttpProxy(Uri hostAddress, Uri[] targets) : base(hostAddress)
        {
            _targets = targets;
            _analysers = new RequestAnalysisEngine();
        }

        public RequestAnalysisEngine AnalyserEngine { get { return _analysers; } }
        
        protected override void Setup(IOwinApplication host)
        {
            host.AddComponent(async c =>
            {
                using (_analysers.Pause())
                {
                    var id = Guid.NewGuid();
                    int i = 0;
                    var tasks = _targets.Select(t => ForwardContext(id, (i++ > 0) ? c.Clone(true) : c, t)).ToList();

                    await Task.WhenAll(tasks);
                }
            });
        }

        private async Task ForwardContext(Guid id, IOwinContext context, Uri target)
        {
            var sw = new Stopwatch();

            sw.Start();

            using (var client = new HttpClient())
            {
                var fwdUri = new Uri(context.RequestUri.Scheme + Uri.SchemeDelimiter + target.Host + ":" + target.Port + context.RequestUri.PathAndQuery);

                if (fwdUri.Scheme != Uri.UriSchemeHttp) return;

                var request = new HttpRequestMessage()
                {
                    RequestUri = fwdUri
                };

                foreach (var header in context.Request.Header.Headers)
                {
                    if (header.Key == "Authorization" ||
                        header.Key.StartsWith("Content") ||
                        header.Key.StartsWith("Accept") ||
                        header.Key == "Cookie" ||
                        header.Key == "User-Agent" ||
                        header.Key.StartsWith("Cache"))
                    {
                        try
                        {
                            request.Headers.Add(header.Key, header.Value);

                            // Console.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error forwarding header: {0} {1}", header.Key, ex.Message);
                        }
                    }
                }

                request.Headers.Add("Host", target.Host);
                request.Method = new HttpMethod(context.Request.Header.HttpVerb);

                if (context.Request.Header.ContentLength > 0)
                {
                    var rc = await context.Request.ToStringAsync();

                    request.Content = new StringContent(rc);

                    //request.Content = new ForwardedRequestContent(context);

                    if (context.Request.Header.ContentEncoding != null)
                        request.Content.Headers.ContentEncoding.Add(context.Request.Header.ContentEncoding.WebName);

                    if (context.Request.Header.ContentMimeType != null)
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.Header.ContentMimeType);
                }

                HttpResponseMessage res = await client.SendAsync(request);

                foreach (var header in res.Headers)
                {
                    // Console.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                    context.Response.Header.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in res.Content.Headers)
                {
                    context.Response.Header.Headers[header.Key] = header.Value.ToArray();
                }

                if (!context.Response.Header.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Header.Headers["Content-Type"] = new[] { "application/json; charset=utf-8" };
                }

                context.Response.Header.StatusCode = (int)res.StatusCode;

                sw.Stop();

                await res.Content.CopyToAsync(context.Response.Content);
                
                var requestBlob = new MemoryStream();

                await context.WriteTo(requestBlob);

                requestBlob.Position = 0;

                await _analysers.EnqueueRequest(new RequestContext(id, fwdUri, context, requestBlob)
                {
                    Elapsed = sw.Elapsed
                });
            }
        }

        private class ForwardedRequestContent : HttpContent
        {
            private readonly MemoryStream _content;

            public ForwardedRequestContent(IOwinContext context)
            {
                _content = new MemoryStream();
                context.Request.Content.CopyTo(_content);
            }

            protected override async Task<Stream> CreateContentReadStreamAsync()
            {
                var stream = new MemoryStream();
                await _content.CopyToAsync(stream);
                await stream.FlushAsync();
                _content.Position = 0;

                return stream;
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _content.Length;
                return true;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                await _content.CopyToAsync(stream);
                await _content.FlushAsync();
                await stream.FlushAsync();
                _content.Position = 0;
            }
        }
    }
}