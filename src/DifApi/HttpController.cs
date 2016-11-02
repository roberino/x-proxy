using LinqInfer.Data.Remoting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DifApi
{
    public class HttpController : HttpAppBase
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

            foreach (var analyserIFace in _proxy.AnalyserEngine.Analysers.Where(a => a is IHasHttpInterface).Cast<IHasHttpInterface>())
            {
                analyserIFace.Register(_api);
            }

            _api.AddErrorHandler((c, e) =>
            {
                c.Response.CreateTextResponse().Write(e.Message);
                return Task.FromResult(true);
            });
        }
    }
}