using LinqInfer.Data.Remoting;
using System;
using System.IO;
using System.Linq;
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
            api.Bind("/source/compare?host1=a&host2=a&path1=b&path2=c&id1=d&id2=e", Verb.Get)
                .To(new
                {
                    host1 = string.Empty,
                    host2 = string.Empty,
                    path1 = string.Empty,
                    path2 = string.Empty,
                    id1 = string.Empty,
                    id2 = string.Empty
                }, x => Compare(x.host1, x.host2, x.path1, x.path2, x.id1, x.id2));

            api.Bind("/source/compare/{host1}/{host2}?path=a", Verb.Get)
                .To(new
                {
                    host1 = string.Empty,
                    host2 = string.Empty,
                    path = string.Empty,
                }, x => Compare(x.host1, x.host2, x.path, x.path));

            api.Bind("/source/compare/{host}?path=a", Verb.Get)
                .To(new
                {
                    host = string.Empty,
                    path = string.Empty,
                }, x => Compare(x.host, x.path));
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            await Task.FromResult(false);
            return requestContext.RequestBlob;
        }

        public async Task<TextTreeComparison> Compare(string host, string path, int max = 5)
        {
            var compare = new TextTreeComparison();

            var srcs = await _store.GetRequestSourceTrees(host, path, max);

            foreach (var tree in srcs)
            {
                compare.Compare(tree);
            }

            return compare;
        }

        public async Task<TextTreeComparison> Compare(string host1, string host2, string path1, string path2, int max = 5)
        {
            var compare = new TextTreeComparison();

            var srcs1 = _store.GetRequestSourceTrees(host1, path1, max);
            var srcs2 = _store.GetRequestSourceTrees(host1, path1, max);

            await Task.WhenAll(srcs1, srcs2);

            foreach (var tree in srcs1.Result.Concat(srcs2.Result))
            {
                compare.Compare(tree);
            }

            return compare;
        }

        public async Task<TextTreeComparison> Compare(string host1, string host2, string path1, string path2, string id1, string id2)
        {
            var compare = new TextTreeComparison();

            var src1 = await _store.GetRequestSourceTree(host1, path1, Guid.Parse(id1));
            var src2 = await _store.GetRequestSourceTree(host2, path2, Guid.Parse(id2));

            compare.Compare(src1, src2);

            return compare;
        }

        public void Dispose()
        {
        }
    }
}