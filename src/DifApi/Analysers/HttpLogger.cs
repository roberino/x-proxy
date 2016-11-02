using LinqInfer.Data.Remoting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DifApi.Analysers
{
    class HttpLogger : IRequestAnalyser, IHasHttpInterface, IDisposable
    {
        private const int DefaultReadSize = 4096 * 6;
        private readonly TextWriter _logger;
        private readonly DirectoryInfo _baseDir;
        private readonly FileInfo _logFile;

        public HttpLogger(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists) baseDir.Create();

            _baseDir = baseDir;

            _logFile = new FileInfo(Path.Combine(_baseDir.FullName, "index.log"));
            _logger = new StreamWriter(new FileStream(_logFile.FullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        }

        public long LogFileSize
        {
            get
            {
                return _logFile.Length;
            }
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/", Verb.Get)
                .To(false, x => Task.FromResult(ListHosts().ToList()));

            api.Bind("/logs/list/{x}", Verb.Get)
                .To((long)0, x => GetRecentRequests(x));

            api.Bind("/logs/url-filter/{pos}?path=y", Verb.Get)
                .To(new
                {
                    pos = (long)0,
                    path = string.Empty
                }, x => GetRequestsByPartialUrl(x.path, x.pos));

            api.Bind("/logs/tree/{x}", Verb.Get)
                .To((long)0, x => GetRequestTree(x));
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            await LogRequest(requestContext);

            return requestContext.RequestBlob;
        }

        public IEnumerable<string> ListHosts()
        {
            return _baseDir.GetDirectories().Select(d => d.Name);
        }

        public async Task<RequestNode> GetRequestTree(long position)
        {
            var root = new RequestNode("/");

            foreach(var item in (await GetRecentRequests(position)).Items)
            {
                var parts = item.OriginUrl.PathAndQuery.Split('/');

                var parent = root;

                foreach (var path in parts.Where(p => !string.IsNullOrEmpty(p)))
                {
                    var node = parent.GetChild(path);

                    node.AverageSizeKb = ((node.RequestCount * node.AverageSizeKb)
                        + ((double)item.ResponseSize / 1024)) / (++node.RequestCount);

                    node.RegisterStatus(item.Status, item.HttpVerb);
                    node.RegisterHost(item.OriginUrl.Host);

                    if (item.Elapsed > node.MaxElapsed)
                    {
                        node.MaxElapsed = item.Elapsed;
                    }

                    if (item.Elapsed < node.MinElapsed || node.MinElapsed == TimeSpan.MinValue)
                    {
                        node.MinElapsed = item.Elapsed;
                    }

                    parent = node;
                }
            }

            return root;
        }

        public Task<ResourceList<LogEntry>> GetRequestsByPartialUrl(string urlPart, long position = -1)
        {
            return GetRecentRequests(position, e => e.OriginUrl.ToString().Contains(urlPart));
        }

        public async Task<ResourceList<LogEntry>> GetRecentRequests(long position = -1, Func<LogEntry, bool> filter = null)
        {
            if (filter == null) filter = _ => true;

            var entries = new List<LogEntry>(256);
            long startPos = 0;

            using (var stream = new FileStream(_logFile.FullName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                if (position > 0 && position < stream.Length)
                {
                    stream.Position = startPos = position;
                }
                else
                {
                    if (stream.Length - DefaultReadSize > 0)
                        stream.Position = startPos = stream.Length - DefaultReadSize;
                }

                using (var reader = new StreamReader(stream))
                {
                    await reader.ReadLineAsync();

                    while (true)
                    {
                        var next = await reader.ReadLineAsync();

                        if (next == null) break;

                        try
                        {
                            var entry = LogEntry.Parse(next);

                            if (filter(entry)) entries.Add(entry);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }

            var resource = new ResourceList<LogEntry>(entries, startPos)
            {
                TotalSize = _logFile.Length
            };

            return resource;
        }

        public async Task LogRequest(RequestContext requestContext)
        {
            _logger.WriteLine(LogEntry.Format(requestContext));

            await _logger.FlushAsync();
        }

        public void Dispose()
        {
            _logger.Close();
            _logger.Dispose();
        }
    }
}