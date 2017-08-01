using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core
{
    public class RequestAnalysisEngine
    {
        private readonly int _maxQueueSize;

        private readonly AutoInvokerV2<Queue<RequestContext>> _worker;
        private readonly IList<IRequestAnalyser> _analysers;

        internal RequestAnalysisEngine(int maxQueueSize = 500)
        {
            _maxQueueSize = maxQueueSize;
            _worker = new AutoInvokerV2<Queue<RequestContext>>(Execute, new Queue<RequestContext>(), s => s.Count > 0);
            _analysers = new List<IRequestAnalyser>();
        }

        public IDisposable Pause()
        {
            return _worker.Pause();
        }

        public IEnumerable<IRequestAnalyser> Analysers { get { return _analysers; } }

        public void Register(IRequestAnalyser analyser)
        {
            _analysers.Add(analyser);
        }

        public Task EnqueueRequest(RequestContext requestContext)
        {
            lock (_worker)
            {
                _worker.State.Enqueue(requestContext);

                if (_worker.State.Count > _maxQueueSize)
                {
                    Console.WriteLine("Queue size exceeded");

                    _worker.Run();
                }
                else
                {
                    _worker.Trigger();
                }
            }

            return Task.FromResult(true);
        }

        private void Execute(Queue<RequestContext> queue)
        {
            RequestContext next;

            if (queue.Count > 0)
            {
                lock (queue) next = queue.Dequeue();

                foreach (var analyser in _analysers)
                {
                    Console.WriteLine("Anaylsing {0} ({1})", next.OriginUrl, analyser);

                    try
                    {
                        var runTask = analyser.Run(next);

                        next = next.Chain(runTask.Result);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                        break;
                    }
                }

                next.Dispose();
            }

            if (queue.Count > 0)
            {
                _worker.Trigger();

                Console.WriteLine("Queue size = {0}", queue.Count);
            }
        }
    }
}