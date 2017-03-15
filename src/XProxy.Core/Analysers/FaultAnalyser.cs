using LinqInfer.Data.Remoting;
using System.IO;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core.Analysers
{
    class FaultAnalyser : IRequestAnalyser, IHasHttpInterface
    {
        public void Register(IHttpApi api)
        {
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            if (requestContext.OwinContext.Response.Header.StatusCode >= 400)
            {
                await Analyse(requestContext);
            }

            return requestContext.RequestBlob;
        }

        private Task Analyse(RequestContext requestContext)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }
    }
}