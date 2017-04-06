using System;
using System.IO;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core
{
    public interface IRequestAnalyser : IDisposable
    {
        Task<RequestContext> Run(RequestContext requestContext);
    }
}
