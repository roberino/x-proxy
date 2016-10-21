using LinqInfer.Data.Remoting;
using LinqInfer.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DifApi
{
    class RequestStore : IDisposable
    {
        private readonly DirectoryInfo _baseDir;
        private readonly IDictionary<string, AutoInvoker<IDocumentIndex>> _indexes;

        public RequestStore(DirectoryInfo baseDir)
        {
            _baseDir = baseDir;
            _indexes = new Dictionary<string, AutoInvoker<IDocumentIndex>>();
        }

        public IEnumerable<string> ListHosts()
        {
            return _baseDir.GetDirectories().Select(d => d.Name);
        }

        public IDocumentIndex GetIndex(string host)
        {
            return GetIndexInvoker(host).State;
        }

        public async Task StoreRequest(IOwinContext context)
        {
            var file = GetPath(context.RequestUri);

            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var mutex = new Mutex(true, file.Name))
            {
                mutex.WaitOne(5000);

                using (var fs = file.OpenWrite())
                {
                    await context.WriteTo(fs);
                }
            }

            using (var fs = file.OpenRead())
            {
                var doc = new TokenisedTextDocument(context.RequestUri.ToString(), TextExtensions.Tokenise(fs));

                var indexInvoker = GetIndexInvoker(context.RequestUri.Host);

                lock (indexInvoker)
                {
                    indexInvoker.State.IndexDocument(doc);
                    indexInvoker.Trigger();
                }
            }
        }

        public void Dispose()
        {
            foreach(var i in _indexes)
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

        public Stream RetrieveRequest(Uri uri)
        {
            var file = GetPath(uri);

            if (file.Exists)
            {
                return file.OpenRead();
            }

            return Stream.Null;
        }

        private FileInfo GetIndexFile(string host)
        {
            return new FileInfo(Path.Combine(_baseDir.FullName, host, "index.dat"));
        }

        private FileInfo GetPath(Uri uri)
        {
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
            .Concat(new[] { name + ".req" }).ToArray());

            return new FileInfo(path);
        }
    }
}