using System;
using LinqInfer.Data.Remoting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace DifApi
{
    class HttpControllerX : HttpAppBase
    {
        private readonly HttpProxy _proxy;

        public HttpControllerX(Uri hostAddress, HttpProxy proxy) : base(hostAddress)
        {
            _proxy = proxy;
        }

        protected override void Setup(IOwinApplication host)
        {
            base.Setup(host);

            //var router = new RoutingHandler();

            //var baseRoute = host.BaseEndpoint.CreateRoute("/");

            host.AddComponent(c =>
            {
                if (c.RequestUri.Scheme != Uri.UriSchemeHttp) return Task.FromResult(0);

                var path = c.RequestUri.PathAndQuery.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();

                if (path.Any())
                {
                    var query = c.RequestUri.Query.Length > 0 ? c.RequestUri.Query.Substring(1) : string.Empty;
                    var isSearch = query.Contains("search=");

                    if (isSearch)
                    {
                        var results = _proxy.Storage.GetIndex(path.First().Split('?').First()).Search(query.Split('=').Last().ToLower());

                        Write(c, results);
                    }
                    else
                    {
                    }
                }
                else
                {
                    Write(c, _proxy.Storage.ListHosts());
                }
                
                return Task.FromResult(0);
            });
        }

        private static void Write(IOwinContext context, object data)
        {
            var jsonWriter = new JsonTextWriter(context.Response.CreateTextResponse());

            var sz = new JsonSerializer();

            sz.Serialize(jsonWriter, data);

            jsonWriter.Flush();

            context.Response.Header.MimeType = "application/json";
        }
    }
}