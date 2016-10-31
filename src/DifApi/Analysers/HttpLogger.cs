using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DifApi.Analysers
{
    class HttpLogger : IDisposable
    {
        private readonly TextWriter _logger;
        private readonly DirectoryInfo _baseDir;

        public HttpLogger(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists) baseDir.Create();

            _baseDir = baseDir;
            _logger = new StreamWriter(new FileStream(Path.Combine(baseDir.FullName, "index.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
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

        public async Task<ResourceList<LogEntry>> GetRecentRequests(long position = -1)
        {
            var entries = new List<LogEntry>(256);
            long startPos = 0;

            using (var stream = new FileStream(Path.Combine(_baseDir.FullName, "index.log"), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                if (stream.Length - 4096 > 0)
                    stream.Position = startPos = stream.Length - 4096;

                using (var reader = new StreamReader(stream))
                {
                    await reader.ReadLineAsync();

                    while (true)
                    {
                        var next = await reader.ReadLineAsync();

                        if (next == null) break;

                        try
                        {
                            entries.Add(LogEntry.Parse(next));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }

            return new ResourceList<LogEntry>(entries, startPos);
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