using LinqInfer.Data.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XProxy.Core.Converters;

namespace XProxy.Core
{
    public class HttpController : HttpAppBase
    {
        private readonly HttpProxy _proxy;
        private readonly IList<IHasHttpInterface> _httpServices;
        private IHttpApi _api;

        public HttpController(Uri hostAddress, HttpProxy proxy) : base(hostAddress)
        {
            _proxy = proxy;
            _httpServices = new List<IHasHttpInterface>();
        }

        protected override void Setup(IOwinApplication host)
        {
            base.Setup(host);

            _api = _host.CreateHttpApi(new JsonSerialiser());

            _api.AddComponent(c =>
            {
                Console.WriteLine(c.RequestUri);
                return Task.FromResult(0);
            }, OwinPipelineStage.Authenticate);

            foreach (var analyserIFace in _proxy
                .AnalyserEngine
                .Analysers
                .Where(a => a is IHasHttpInterface)
                .Cast<IHasHttpInterface>()
                .Concat(_httpServices))
            {
                analyserIFace.Register(_api);
            }

            _api.AddErrorHandler((c, e) =>
            {
                c.Response.CreateTextResponse().Write(e.ToString());
                return Task.FromResult(true);
            });
        }

        public void RegisterHttpService(IHasHttpInterface httpService)
        {
            _httpServices.Add(httpService);
        }
    }
}