using LinqInfer.Data.Remoting;
using LinqInfer.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Core.Converters;
using XProxy.Core.Models;

namespace XProxy.Core.Analysers
{
    class RequestStore : IRequestAnalyser, IHasHttpInterface, IRequestStore
    {
        private readonly DirectoryInfo _baseDir;
        private readonly IDictionary<string, AutoInvokerV1<IDocumentIndex>> _indexes;
        private readonly HttpLogger _logger;

        public RequestStore(DirectoryInfo baseDir, HttpLogger logger)
        {
            _baseDir = baseDir;

            if (!_baseDir.Exists) _baseDir.Create();

            _indexes = new Dictionary<string, AutoInvokerV1<IDocumentIndex>>();
            _logger = logger;
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/source?host=a&path=b&id=c", Verb.Get)
                .To(new
                {
                    host = string.Empty,
                    path = string.Empty,
                    id = string.Empty
                }, x => GetRequestSource(x.host, x.path, Guid.Parse(x.id)));
        }

        public async Task<RequestContext> Run(RequestContext requestContext)
        {
            var file = GetPath(requestContext);
            var fileTree = new FileInfo(file.FullName + ".json");

            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var mutex = new Mutex(true, file.Name))
            {
                mutex.WaitOne(5000);

                using (var fs = file.OpenWrite())
                {
                    await requestContext.CopyToAsync(fs);
                }
            }

            using (var mutex = new Mutex(true, file.Name))
            {
                mutex.WaitOne(5000);

                using (var fs = fileTree.OpenWrite())
                {
                    var tree = OwinContextToTextTree.Create(requestContext);

                    await tree.WriteAsync(fs);
                }
            }

            return requestContext;
        }

        public async Task<SourceFile> GetRequestSource(string host, string path, Guid id)
        {
            var uri = new Uri(UriHelper.UriSchemeHttp + UriHelper.SchemeDelimiter + host + path);
            var fileInfo = GetPath(uri, id);

            if (!fileInfo.Exists)
            {
                fileInfo = fileInfo.Directory.GetFiles("*.req").FirstOrDefault(f => f.Name.StartsWith(id.ToString()));

                if (fileInfo == null)
                {
                    return null;
                }
            }

            using (var stream = fileInfo.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var source = new SourceFile(uri, id)
                {
                    Content = await reader.ReadToEndAsync()
                };

                return source;
            }
        }

        public async Task<IList<TextTree>> GetRequestSourceTrees(string host, string path, int max = 5)
        {
            var paths = FindPaths(new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + host + path), ".json");

            var streams = paths.OrderByDescending(p => p.LastAccessTimeUtc).Where(p => p.Length > 0).Take(max).Select(p => p.OpenRead()).ToList();

            var tasks = streams.Select(s => TextTree.LoadAsync(s)).ToList();

            await Task.WhenAll(tasks);

            foreach (var stream in streams) stream.Dispose();

            return tasks.Select(t => t.Result).ToList();
        }

        public async Task<TextTree> GetRequestSourceTree(string host, string path, Guid id)
        {
            var uri = new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + host + path);
            var fileInfo = GetPath(uri, id, ".json");

            if (!fileInfo.Exists)
            {
                fileInfo = fileInfo.Directory.GetFiles("*.json").FirstOrDefault(f => f.Name.StartsWith(id.ToString()));

                if (fileInfo == null)
                {
                    return null;
                }
            }

            using (var stream = fileInfo.OpenRead())
            {
                return await TextTree.LoadAsync(stream);
            }
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
            return GetPath(context.OriginUrl, context.Id);
        }

        private IEnumerable<FileInfo> FindPaths(Uri uri, string ext = null)
        {
            var examplePath = GetPath(uri, Guid.Empty, ext);

            var path = examplePath.Name.Split('_').Last();

            return examplePath
                .Directory
                .GetFiles("*" + examplePath.Extension)
                .Where(f => f.Name.EndsWith(path))
                .OrderByDescending(f => f.LastWriteTimeUtc);
        }

        private FileInfo GetPath(Uri uri, Guid id, string ext = null)
        {
            return GetPath(_baseDir, uri, id, ext);
        }

        public static FileInfo GetPath(DirectoryInfo baseDir, Uri uri, Guid? id = null, string ext = null)
        {
            // TODO: NOT REMOVING: ? &

            var query = uri.PathAndQuery.IndexOf('?');
            var paths = (query == -1 ? uri.PathAndQuery : uri.PathAndQuery.Substring(0, query)).Split('/');
            var invalidNameChars = Path.GetInvalidFileNameChars();
            var invalidPathChars = Path.GetInvalidPathChars();
            var isExtensionless = !paths.Last().Contains(".");

            var cleanPathArray = paths.Take(paths.Length - (isExtensionless ? 0 : 1))
                .Select(p =>
                    new string(p.Select(c => invalidPathChars.Contains(c) ? '_' : c).ToArray()));

            var name = !isExtensionless ? new string(paths.Last().Select(c => invalidNameChars.Contains(c) ? '_' : c).ToArray()) : "index";

            var path = Path.Combine(
             new[] { baseDir.FullName, uri.Host }
            .Concat(cleanPathArray)
            .Concat(new[] { (id == null ? null : id.ToString() + '_') + name + ".req" + ext }).ToArray());

            return new FileInfo(path);
        }
    }
}