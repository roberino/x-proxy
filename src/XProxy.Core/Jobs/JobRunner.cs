using System;
using System.Collections.Generic;

namespace XProxy.Core.Jobs
{
    public class JobRunner : IDisposable
    {
        private readonly IList<AutoInvokerV2<ExecutionContext>> _jobs;
        private readonly ExecutionContext _context;
        private bool _sleeping;

        public JobRunner(ExecutionContext context)
        {
            _context = context;
            _sleeping = true;
            _jobs = new List<AutoInvokerV2<ExecutionContext>>();
        }

        public ExecutionContext Context { get { return _context; } }

        public void Register(ContinuousJob job)
        {
            _jobs.Add(new AutoInvokerV2<ExecutionContext>(c => job.Execute(c), _context, _ => !_sleeping));
        }

        public void Start()
        {
            _sleeping = false;
            foreach (var job in _jobs)
            {
                job.Trigger();
            }
        }

        public void Stop()
        {
            _sleeping = true;
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