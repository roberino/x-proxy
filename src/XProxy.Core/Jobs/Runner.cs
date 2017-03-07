using System;
using System.Collections.Generic;

namespace XProxy.Core.Jobs
{
    public class Runner : IDisposable
    {
        private readonly IList<AutoInvokerV2<ExecutionContext>> _jobs;
        private readonly ExecutionContext _context;

        public Runner(ExecutionContext context)
        {
            _context = context;
            _jobs = new List<AutoInvokerV2<ExecutionContext>>();
        }

        public void Register(ContinuousJob job)
        {
            _jobs.Add(new AutoInvokerV2<ExecutionContext>(c => job.Execute(c), _context));
        }

        public void Start()
        {
            foreach(var job in _jobs)
            {
                job.Run();
            }
        }

        public void Stop()
        {
            foreach (var job in _jobs)
            {
                job.Pause();
            }
        }

        public void Dispose()
        {
            lock (_jobs)
            {
                Stop();

                foreach (var job in _jobs)
                {
                    job.Dispose();
                }

                _jobs.Clear();
            }
        }
    }
}