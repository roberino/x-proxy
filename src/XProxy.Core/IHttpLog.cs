using System;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core
{
    public interface IHttpLog
    {
        Task<ResourceList<LogEntry>> GetRecentRequests(long position = -1, Func<LogEntry, bool> filter = null);
        Task<ResourceList<LogEntry>> GetRequestsByPartialUrl(string urlPart, long position = -1);
        Task<RequestNode> GetRequestTree(long position);
        ResourceList<string> ListHosts();
    }
}