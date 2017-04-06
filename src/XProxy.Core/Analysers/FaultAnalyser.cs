using LinqInfer.Data.Remoting;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core.Analysers.Faults
{
    public class FaultAnalyser : IRequestAnalyser, IHasHttpInterface
    {
        private readonly SessionStore _sessionStore;

        public FaultAnalyser(SessionStore sessionStore)
        {
            _sessionStore = sessionStore;
        }

        public void Register(IHttpApi api)
        {
        }

        public async Task<RequestContext> Run(RequestContext requestContext)
        {
            await Analyse(requestContext);

            return requestContext;
        }

        private async Task Analyse(RequestContext requestContext)
        {
            var fault = new HypotheticalFault(requestContext.OriginUrl);

            var file = RequestStore.GetPath(_sessionStore.BaseStorageDirectory, requestContext.OriginUrl, null, ".hypo");

            if (file.Exists)
            {
                using (var fs = file.OpenRead())
                {
                    await fault.ReadAsync(fs);
                }

                // Look at HTTP status
                // Compare request context to past requests - check diffs
                // Compare to other hosts?
            }

            using (var fs = file.OpenWrite())
            {
                await fault.WriteAsync(fs);
            }
        }

        public void Dispose()
        {
        }
    }
}