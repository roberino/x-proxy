using System;
using System.Linq;
using System.Threading.Tasks;
using XProxy.Core.Events;
using XProxy.Core.Models;

namespace XProxy.Core.Jobs
{
    class DiffEngine : ContinuousJob
    {
        private DateTime? _lastRuntime;

        public override async Task Execute(ExecutionContext context)
        {
            if (context.SessionStore.LastWriteTime > DateTime.UtcNow.AddSeconds(-10)) return;

            var lrt = _lastRuntime.GetValueOrDefault(DateTime.UtcNow.AddMinutes(-1));

            var recent = await context.HttpLogs.GetRecentRequests(-1, e => e.Date >= lrt);

            if (recent.Items.Count <= 5) return;

            _lastRuntime = DateTime.UtcNow;

            foreach (var reqPattern in recent.Items.GroupBy(r => r.OriginPath))
            {
                var reqPatternList = reqPattern.ToList();
                var hosts = reqPattern.Select(p => p.OriginHost).Distinct().ToList();

                if (reqPatternList.Count > 1 && hosts.Count > 1)
                {
                    var compare = new TextTreeComparison();

                    foreach (var req in reqPatternList)
                    {
                        var tree = await context.RequestStore.GetRequestSourceTree(req.OriginHost, req.OriginPath, Guid.Parse(req.Id));

                        compare.Compare(tree);
                    }

                    if (compare.TotalDiffs > 0)
                    {
                        var uri = new Uri(context.ServiceEndpoint, string.Format("/source/compare/{0}/{1}?path={2}&flatten=true", hosts[0], hosts[1], reqPattern.Key));
                        var ev = new PublishedEvent(reqPattern.Key, uri)
                        {
                            EventType = "Difference"
                        };
                        
                        ev.Data["totalDiffs"] = compare.TotalDiffs;
                        ev.Data["path"] = reqPattern.Key;

                        int i = 0;

                        foreach(var host in hosts)
                        {
                            ev.Data["host_" + (i++)] = host;
                        }

                        await context.EventDispatcher.DispatchEvent(ev);
                    }
                }
            }
        }
    }
}