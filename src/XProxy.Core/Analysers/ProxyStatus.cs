using LinqInfer.Data.Remoting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Core.Analysers
{
    class ProxyStatus : IRequestAnalyser
    {
        private readonly IServer _server;

        public ProxyStatus(IServer server)
        {
            _server = server;
        }

        public void Dispose()
        {
        }

        public Task<Stream> Run(RequestContext requestContext)
        {
            Console.WriteLine(_server.Status);

            return Task.FromResult(requestContext.RequestBlob);
        }
    }
}
