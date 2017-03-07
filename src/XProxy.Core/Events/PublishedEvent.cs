using System;

namespace XProxy.Core.Events
{
    public class PublishedEvent
    {
        public PublishedEvent(string title, Uri url = null)
        {
            Id = Guid.NewGuid();
            Published = DateTime.UtcNow;
            Title = title;
        }

        public Guid Id { get; protected set; }

        public DateTime Published { get; protected set; }

        public string Title { get; protected set; }

        public Uri Url { get; protected set; }
    }
}