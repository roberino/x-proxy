using LinqInfer.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqInfer.Data.Remoting;

namespace DifApi.Analysers
{
    class TextIndexer : IRequestAnalyser, IHasHttpInterface
    {
        private readonly DirectoryInfo _baseDir;
        private readonly HttpLogger _logger;
        private readonly IDictionary<string, AutoInvoker<IDocumentIndex>> _indexes;

        public TextIndexer(DirectoryInfo baseDir, HttpLogger logger)
        {
            _baseDir = baseDir;
            _logger = logger;
            _indexes = new Dictionary<string, AutoInvoker<IDocumentIndex>>();
        }

        public Task<Stream> Run(RequestContext requestContext)
        {
            var tokens = TextExtensions.Tokenise(requestContext.RequestBlob).ToList();
            var doc = new TokenisedTextDocument(requestContext.OriginUrl.ToString(), tokens);

            var indexInvoker = GetIndexInvoker(requestContext.OriginUrl.Host);

            lock (indexInvoker)
            {
                indexInvoker.State.IndexDocument(doc);
                indexInvoker.Trigger();
            }

            return Task.FromResult(requestContext.RequestBlob);
        }

        public IEnumerable<string> ListHosts()
        {
            return _baseDir.GetDirectories().Select(d => d.Name);
        }

        public IDocumentIndex GetIndex(string host)
        {
            return GetIndexInvoker(host).State;
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/{host}?search=x", Verb.Get, r => r.RequestUri.Scheme == Uri.UriSchemeHttp)
                .To(new
                {
                    host = string.Empty,
                    search = string.Empty
                }, x =>
                {
                    var results = GetIndex(x.host).Search(x.search.ToLower());

                    return Task.FromResult(results);
                });

            api.Bind("/logs/search?q=x", Verb.Get, r => r.RequestUri.Scheme == Uri.UriSchemeHttp)
                .To(string.Empty, QueryAsync);
        }

        private async Task<ResourceList<LogEntry>> QueryAsync(string q)
        {
            var allResults = new List<LogEntry>();

            foreach (var host in _logger.ListHosts())
            {
                var results = GetIndex(host).Search(q.ToLower()).GroupBy(h => h.DocumentKey).ToDictionary(h => h.Key, h => h.Max(x => x.Score));

                var matches = await _logger.GetRecentRequests(-1, e => results.ContainsKey(e.OriginUrl.ToString()));

                foreach (var match in matches.Items)
                {
                    allResults.Add(match.SetOrder((float)results[match.OriginUrl.ToString()]));
                }
            }

            return new ResourceList<LogEntry>(allResults.OrderByDescending(r => r.Order));
        }

        public void Dispose()
        {
            foreach (var i in _indexes)
            {
                i.Value.Dispose();
            }
        }

        private AutoInvoker<IDocumentIndex> GetIndexInvoker(string host)
        {
            Contract.Requires(host != null);

            AutoInvoker<IDocumentIndex> indexInvoker;

            var key = host.ToLower();

            if (!_indexes.TryGetValue(key, out indexInvoker))
            {
                var file = GetIndexFile(host);

                _indexes[key] = indexInvoker = new AutoInvoker<IDocumentIndex>(i => SaveIndex(i, host), new TokenisedTextDocument[] { }.CreateIndex());

                if (file.Exists)
                {
                    using (var fileStream = file.OpenRead())
                    {
                        indexInvoker.State.Load(fileStream);
                    }
                }
            }

            return indexInvoker;
        }

        private void SaveIndex(IDocumentIndex index, string host)
        {
            var state = new
            {
                Index = index,
                Host = host
            };

            {
                var indexFile = GetIndexFile(state.Host);

                lock (state.Index)
                {
                    using (var indexStream = indexFile.OpenWrite())
                    {
                        state.Index.Save(indexStream);
                    }
                }

                Console.WriteLine("Index updated - " + host);
            }
        }

        private FileInfo GetIndexFile(string host)
        {
            return new FileInfo(Path.Combine(_baseDir.FullName, host, "index.dat"));
        }
    }
}