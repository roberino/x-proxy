using System;
using System.IO;
using System.Threading.Tasks;

namespace XProxy.Core
{
    public interface IRequestAnalyser : IDisposable
    {
        Task<Stream> Run(RequestContext requestContext);
    }
}
