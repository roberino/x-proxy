using System;
using System.Collections.Generic;

namespace XProxy.Core.Events
{
    public class PublishedEvent
    {
        protected PublishedEvent()
        {
            Data = new Dictionary<string, object>();
        }

        public PublishedEvent(string title, Uri url = null)
        {
            Id = Guid.NewGuid().ToString();
            Published = DateTime.UtcNow;
            Title = title;
            Url = url;
            Data = new Dictionary<string, object>();
        }

        public PublishedEvent(string id, string title, Uri url = null)
        {
            Id = id;
            Published = DateTime.UtcNow;
            Title = title;
            Url = url;
            Data = new Dictionary<string, object>();
        }

        public string Id { get; set; }

        public DateTime Published { get; set; }

        public string Title { get; set; }

        public string EventType { get; set; }

        public Uri Url { get; set; }

        public IDictionary<string, object> Data { get; set; }
    }
}