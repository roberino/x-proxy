using System.Collections.Generic;
using System.Linq;

namespace DifApi.Analysers
{
    public class ResourceList<T>
    {
        public ResourceList(IEnumerable<T> items, long positionStart = 0)
        {
            Items = items.ToList();
            StartPosition = positionStart;
        }

        public IList<T> Items { get; private set; }
        public long StartPosition { get; private set; }
    }
}