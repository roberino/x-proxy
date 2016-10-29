using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DifApi
{
    public class RequestAnalysisEngine
    {
        private readonly int _maxQueueSize;

        private readonly AutoInvoker<Queue<RequestContext>> _worker;
        private readonly IList<IRequestAnalyser> _analysers;

        public RequestAnalysisEngine(int maxQueueSize = 100)
        {
            _maxQueueSize = maxQueueSize;
            _worker = new AutoInvoker<Queue<RequestContext>>(Execute, new Queue<RequestContext>());
            _analysers = new List<IRequestAnalyser>();
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

            while (queue.Count > 0)
            {
                lock (queue) next = queue.Dequeue();

                Stream data = null;

                foreach (var analyser in _analysers)
                {
                    Console.WriteLine("Anaylsing {0} ({1})", next.OriginUrl, analyser);

                    try
                    {
                        var runTask = analyser.Run(next);

                        data = runTask.Result;

                        next = next.Chain(data);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                        break;
                    }
                }

                next.Dispose();
            }
        }
    }
}