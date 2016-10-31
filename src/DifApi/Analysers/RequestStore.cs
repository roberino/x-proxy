using LinqInfer.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqInfer.Data.Remoting;
using System.Xml;

namespace DifApi.Analysers
{
    class RequestStore : IRequestAnalyser, IHasHttpInterface
    {
        private readonly DirectoryInfo _baseDir;
        private readonly IDictionary<string, AutoInvoker<IDocumentIndex>> _indexes;
        private readonly HttpLogger _logger;

        public RequestStore(DirectoryInfo baseDir)
        {
            _baseDir = baseDir;

            if (!_baseDir.Exists) _baseDir.Create();

            _indexes = new Dictionary<string, AutoInvoker<IDocumentIndex>>();
            _logger = new HttpLogger(_baseDir);
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/", Verb.Get)
                .To(false, x => Task.FromResult(ListHosts().ToList()));

            api.Bind("/logs/{x}", Verb.Get)
                .To((long)0, x => _logger.GetRecentRequests(x));

            api.Bind("/tree/{x}", Verb.Get)
                .To((long)0, x => _logger.GetRequestTree(x));
        }

        public IEnumerable<string> ListHosts()
        {
            return _baseDir.GetDirectories().Select(d => d.Name);
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            var file = GetPath(requestContext);

            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var mutex = new Mutex(true, file.Name))
            {
                mutex.WaitOne(5000);

                using (var fs = file.OpenWrite())
                {
                    await requestContext.RequestBlob.CopyToAsync(fs);
                }
            }

            await _logger.LogRequest(requestContext);

            return requestContext.RequestBlob;
        }

        public void Dispose()
        {
            _logger.Dispose();

            foreach(var i in _indexes)
            {
                i.Value.Dispose();
            }
        }

        private FileInfo GetPath(RequestContext context)
        {
            var uri = context.OriginUrl;
            var paths = uri.PathAndQuery.Split('/');
            var invalidNameChars = Path.GetInvalidFileNameChars();
            var invalidPathChars = Path.GetInvalidPathChars();
            var isExtensionless = !paths.Last().Contains(".");

            var cleanPathArray = paths.Take(paths.Length - (isExtensionless ? 0 : 1))
                .Select(p =>
                    new string(p.Select(c => invalidPathChars.Contains(c) ? '_' : c).ToArray()));

            var name = !isExtensionless ? new string(paths.Last().Select(c => invalidNameChars.Contains(c) ? '_' : c).ToArray()) : "index";

            var path = Path.Combine(
             new [] { _baseDir.FullName, uri.Host }
            .Concat(cleanPathArray)
            .Concat(new[] { context.Id.ToString() + '_' + name + ".req" }).ToArray());

            return new FileInfo(path);
        }
    }
}