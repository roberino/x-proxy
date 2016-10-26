using System;
using LinqInfer.Data.Remoting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace DifApi
{
    class HttpController : HttpAppBase
    {
        private readonly HttpProxy _proxy;
        private IHttpApi _api;

        public HttpController(Uri hostAddress, HttpProxy proxy) : base(hostAddress)
        {
            _proxy = proxy;
        }

        protected override void Setup(IOwinApplication host)
        {
            base.Setup(host);

            _api = _host.CreateHttpApi(new JsonSerialiser());

            _api.Bind("/{host}?search=x", Verb.Get, r => r.Request.Header.Query.ContainsKey("search"))
                .To(new
                {
                    host = string.Empty,
                    search = string.Empty
                }, x =>
                {
                    var results = _proxy.Storage.GetIndex(x.host).Search(x.search);

                    return Task.FromResult(results);
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