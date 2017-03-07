using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XProxy.Core.Events
{
    public interface IEventDispatcher : IDisposable
    {
        Task DispatchEvent(PublishedEvent ev);
        Task<IEnumerable<PublishedEvent>> Receieve(string clientId, DateTime? startDate = null);
    }
}