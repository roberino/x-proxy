using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqInfer.Data.Remoting;
using XProxy.Core.Converters;
using XProxy.Core.Models;

namespace XProxy.Core.Events
{
    public class FileSystemEventDispatcher : IDisposable, IEventDispatcher, IHasHttpInterface
    {
        private readonly DirectoryInfo _baseDir;

        public FileSystemEventDispatcher(DirectoryInfo baseDir = null)
        {
            _baseDir = new DirectoryInfo(Path.Combine(baseDir.FullName, "_events"));

            if (!_baseDir.Exists) _baseDir.Create();
        }

        public async Task DispatchEvent(PublishedEvent ev)
        {
            var evf = new FileInfo(Path.Combine(_baseDir.FullName, ev.Id.ToString() + ".ev.json"));

            using (var fs = evf.OpenWrite())
            {
                var sz = new JsonSerialiser();

                await sz.Serialise(ev, Encoding.UTF8, sz.SupportedMimeTypes.First(), fs);
            }
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/events/{clientId}").To(new { clientId = "" }, async a => new ResourceList<PublishedEvent>(await Receieve(a.clientId)));
        }

        public async Task<IEnumerable<PublishedEvent>> Receieve(string clientId, DateTime? startDate = null)
        {
            var sd = startDate.GetValueOrDefault(DateTime.UtcNow.AddDays(-1));
            var sz = new JsonSerialiser();
            var items = new List<PublishedEvent>();

            foreach (var file in _baseDir.GetFiles("*.ev.json").Where(f => f.LastWriteTimeUtc > sd))
            {
                using (var fs = file.OpenRead())
                {
                    var ev = await sz.Deserialise<PublishedEvent>(fs, Encoding.UTF8, sz.SupportedMimeTypes.First());

                    items.Add(ev);
                }
            }

            return items;
        }

        public void Dispose()
        {
        }
    }
}