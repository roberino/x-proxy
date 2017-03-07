using System;

namespace XProxy.Core.Models
{
    public class SourceFile
    {
        public SourceFile(Uri url, Guid id)
        {
            Url = url;
            Id = id;
        }

        public Guid Id { get; private set; }
        public Uri Url { get; private set; }
        public string Content { get; set; }
    }
}
