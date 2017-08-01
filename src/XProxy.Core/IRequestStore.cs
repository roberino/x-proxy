using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core
{
    public interface IRequestStore
    {
        Task<SourceFile> GetRequestSource(string host, string path, Guid id);
        Task<TextTree> GetRequestSourceTree(string host, string path, Guid id);
        Task<IList<TextTree>> GetRequestSourceTrees(string host, string path, int max = 5);
    }
}