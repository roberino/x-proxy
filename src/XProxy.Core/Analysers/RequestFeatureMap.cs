using LinqInfer.Data.Remoting;
using LinqInfer.Learning;
using LinqInfer.Maths;
using LinqInfer.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XProxy.Core.Analysers
{
    class RequestFeatureMap : IRequestAnalyser, IHasHttpInterface
    {
        private const string CacheHeader = "Cache-Control";

        private readonly HttpLogger _logger;
        private readonly EnglishDictionary _dict;
        private readonly FileInfo _featureIndex;
        private readonly TextWriter _featureIndexStream;

        public RequestFeatureMap(DirectoryInfo baseDir, HttpLogger logger)
        {
            _logger = logger;
            _dict = new EnglishDictionary();
            _featureIndex = new FileInfo(Path.Combine(baseDir.FullName, "features.dat"));

            if (!_featureIndex.Directory.Exists) _featureIndex.Directory.Create();

            _featureIndexStream = new StreamWriter(new FileStream(_featureIndex.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite), Encoding.ASCII, 1024, false);
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/logs/features/map/{x}").To(10, async x =>
            {
                var data = await Read();

                var map = await data.AsQueryable().CreatePipeline().ToSofm(x).ExecuteAsync();

                return map;
            });
        }

        public async Task<Stream> Run(RequestContext requestContext)
        {
            var data = await _logger.GetRequestsByPartialUrl(requestContext.OriginUrl.ToString());

            var vector = new RequestVector()
            {
                Url = requestContext.OriginUrl,
                ErrorCount = requestContext.OwinContext.Response.Header.StatusCode.GetValueOrDefault(0) >= 400 ? 1 : 0,
                IsCacheable = IsCacheable(requestContext.OwinContext),
                IsPublic = IsPublic(requestContext.OwinContext),
                IsHtml = requestContext.OwinContext.Response.Header.MimeType.Contains("text/html"),
                IsJson = requestContext.OwinContext.Response.Header.MimeType.Contains("json"),
                PathCount = requestContext.OriginUrl.PathAndQuery.Split('/').Count(),
                RequestCount = data.Items.Count,
                ResponseSize = data.Items.Select(r => r.ResponseSize / 1024d).Mean(),
                ResponseTime = data.Items.Select(r => r.ElapsedMilliseconds).Mean(),
                UniqueRefererCount = data.Items.Where(r => r.RefererUrl != null).Select(r => r.RefererUrl).Distinct().Count()
            };

            var tokens = (await Tokenise(requestContext)).ToList();

            vector.TokenCount = tokens.Count;
            vector.SemanticTokenRatio = tokens.Where(t => t.Type == TokenType.Word && _dict.IsWord(t.Text)).Count() / (double)tokens.Count;
            vector.NumericTokenRatio = tokens.Where(t => t.Type == TokenType.Number).Count() / (double)tokens.Count;

            await Write(vector);

            return requestContext.RequestBlob;
        }

        private async Task<IList<RequestVector>> Read()
        {
            using (var fs = new FileStream(_featureIndex.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var items = new Dictionary<Uri, RequestVector>();

                using (var reader = new StreamReader(fs))
                {
                    while (true)
                    {
                        var next = await reader.ReadLineAsync();

                        if (next == null) break;

                        var obj = JsonConvert.DeserializeObject<RequestVector>(next);

                        items[obj.Url] = obj;
                    }
                }

                return items.Values.ToList();
            }
        }

        private async Task Write(RequestVector vector)
        {
            var json =  JsonConvert.SerializeObject(vector, Formatting.None);

            await _featureIndexStream.WriteLineAsync(json);

            await _featureIndexStream.FlushAsync();
        }

        private async Task<IEnumerable<IToken>> Tokenise(RequestContext requestContext)
        {
            using (var ms = new MemoryStream())
            {
                await requestContext.RequestBlob.CopyToAsync(ms);

                requestContext.RequestBlob.Position = 0;
                ms.Position = 0;

                if (ms.Length == 0) return Enumerable.Empty<IToken>();

                switch (requestContext.OwinContext.Response.Header.MimeType)
                {
                    case "application/json":
                    case "text/html":
                    default:
                        return ms.Tokenise().ToList();
                }
            }
        }

        private IEnumerable<IToken> TokeniseJson(Stream jsonStream)
        {
            throw new NotImplementedException();
        }

        private bool IsPublic(IOwinContext context)
        {
            if (context.Response.Header.Headers.ContainsKey(CacheHeader))
            {
                return context.Response.Header.Headers[CacheHeader].Any(h => h.Contains("public"));
            }

            return false;
        }

        private bool IsCacheable(IOwinContext context)
        {
            if (context.Response.Header.Headers.ContainsKey(CacheHeader))
            {
                return !context.Response.Header.Headers[CacheHeader].Any(h => Regex.IsMatch(h, "max-age=0(,|$)"));
            }

            return false;
        }

        public void Dispose()
        {
            _featureIndexStream.Close();
            _featureIndexStream.Dispose();
        }
    }
}
