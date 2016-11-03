using System;
using System.Collections.Generic;
using System.Linq;

namespace XProxy.Core.Analysers
{
    public class RequestNode
    {
        private readonly RequestNode _parent;

        public RequestNode(string path, RequestNode parent = null)
        {
            _parent = parent;

            Path = path;
            Children = new Dictionary<string, RequestNode>();
            Statuses = Enumerable.Empty<int>();
            Verbs = Enumerable.Empty<string>();
            Hosts = Enumerable.Empty<string>();
            MinElapsed = TimeSpan.MinValue;
        }
        public IDictionary<string, RequestNode> Children { get; private set; }
        public bool IsLeaf { get { return !Children.Any(); } }
        public string Path { get; private set; }

        public string FullPath
        {
            get
            {
                return (_parent == null ? string.Empty : (_parent.FullPath == "/" ? "" : _parent.FullPath) + "/") + Path;
            }
        }

        public double AverageSizeKb { get; set; }
        public long RequestCount { get; set; }
        public IEnumerable<int> Statuses { get; private set; }
        public IEnumerable<string> Verbs { get; private set; }
        public IEnumerable<string> Hosts { get; private set; }

        public TimeSpan MaxElapsed { get; set; }
        public TimeSpan MinElapsed { get; set; }

        public void RegisterStatus(int status, string verb)
        {
            Statuses = Statuses.Concat(new[] { status }).Distinct().ToList();
            Verbs = Verbs.Concat(new[] { verb }).Distinct().ToList();
        }

        public void RegisterHost(string host)
        {
            Hosts = Hosts.Concat(new[] { host }).Distinct().ToList();
        }

        public RequestNode GetChild(string path)
        {
            RequestNode node;

            if (!Children.TryGetValue(path, out node))
            {
                node = new RequestNode(path, this);
                Children[path] = node;
            }

            return node;
        }
    }
}