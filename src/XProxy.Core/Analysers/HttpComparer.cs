using LinqInfer.Data.Remoting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace XProxy.Core.Analysers
{
    class HttpComparer : IRequestAnalyser, IHasHttpInterface
    {
        private readonly RequestStore _store;

        public HttpComparer(RequestStore store)
        {
            _store = store;
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/source/compare?host1=a&host2=a&path=b&id1=c&id2=d", Verb.Get)
                .To(new
                {
                    host1 = string.Empty,
                    host2 = string.Empty,
                    path = string.Empty,
                    id1 = string.Empty,
                    id2 = string.Empty
                }, x => Compare(x.host1, x.host2, x.path, x.id1, x.id2));
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            await Task.FromResult(false);
            return requestContext.RequestBlob;
        }

        public async Task<TextTreeComparison> Compare(string host1, string host2, string path, string id1, string id2)
        {
            var compare = new TextTreeComparison();

            var src1 = await _store.GetRequestSourceTree(host1, path, Guid.Parse(id1));
            var src2 = await _store.GetRequestSourceTree(host2, path, Guid.Parse(id2));

            compare.Compare(src1, src2);

            return compare;
        }

        public void Dispose()
        {
        }
    }
}