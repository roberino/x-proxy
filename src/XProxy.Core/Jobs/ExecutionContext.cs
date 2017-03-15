using System;
using System.IO;
using XProxy.Core.Events;

namespace XProxy.Core.Jobs
{
    public class ExecutionContext
    {
        public DirectoryInfo BaseDirectory { get; internal set; }

        public IHttpLog HttpLogs { get; internal set; }

        public IRequestStore RequestStore { get; internal set; }

        public IEventDispatcher EventDispatcher { get; internal set; }

        public TextWriter Logger { get; internal set; }

        public Uri ServiceEndpoint { get; internal set; }
    }
}
