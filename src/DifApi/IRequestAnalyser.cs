using System;
using System.IO;
using System.Threading.Tasks;

namespace DifApi
{
    public interface IRequestAnalyser : IDisposable
    {
        Task<Stream> Run(RequestContext requestContext);
    }
}
