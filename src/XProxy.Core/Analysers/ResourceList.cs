using System.Collections.Generic;
using System.Linq;

namespace XProxy.Core.Analysers
{
    public class ResourceList<T>
    {
        public ResourceList(IEnumerable<T> items, long positionStart = 0)
        {
            Items = items.ToList();
            StartPosition = positionStart;
            Attributes = new Dictionary<string, object>();
        }

        public ResourceList(long positionStart = 0)
        {
            Items = new List<T>();
            Attributes = new Dictionary<string, object>();
            StartPosition = positionStart;
        }

        public IDictionary<string, object> Attributes { get; private set; }
        public IList<T> Items { get; private set; }
        public long StartPosition { get; private set; }
        public long TotalSize { get; internal set; }
    }
}