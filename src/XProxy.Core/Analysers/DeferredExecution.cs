using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Core.Analysers
{
    /// <summary>
    /// Defers execution of a task so that when multiple actions
    /// trigger the same task, the task execution will be deferred
    /// for a specified time and only run when activity ceases.
    /// </summary>
    class DeferredExecution : IDisposable
    {
        private readonly IDictionary<string, DeferredTask> _actions;
        private readonly ManualResetEvent _waitHandle;
        private readonly ManualResetEvent _runHandle;
        private readonly Task _worker;
        private bool _isDisposed;

        public DeferredExecution(Func<Exception, bool> errorHandler = null)
        {
            _actions = new Dictionary<string, DeferredTask>();
            _waitHandle = new ManualResetEvent(false);
            _runHandle = new ManualResetEvent(false);

            _worker = Task.Factory.StartNew(async () =>
            {
                while (!_isDisposed)
                {
                    var active = _actions.Values.Where(a => a.CanBeRun).ToList();

                    if (active.Any())
                    {
                        try
                        {
                            _runHandle.Reset();

                            var activeTasks = active.Select(a => a.Run()).ToList();

                            await Task.WhenAll(activeTasks);
                        }
                        catch (Exception ex)
                        {
                            if (!(errorHandler?.Invoke(ex)).GetValueOrDefault())
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            _runHandle.Set();
                        }

                        lock (_actions)
                        {
                            foreach (var item in active.Where(a => a.WasExecuted))
                            {
                                _actions.Remove(item.Id);
                            }
                        }
                    }

                    if (_isDisposed) break;

                    _waitHandle.WaitOne(5000);
                }
            });
        }

        public void ExecuteOncePerId(string id, Func<Task> action, TimeSpan delay)
        {
            Contract.Assert(id != null);
            Contract.Assert(action != null);

            lock (_actions)
            {
                DeferredTask task;

                if (!_actions.TryGetValue(id, out task) || task.WasExecuted)
                {
                    _actions[id] = task = new DeferredTask() { Id = id };
                }

                task.Task = action;
                task.Modified = DateTime.UtcNow;
                task.Delay = delay;
            }

            _waitHandle.Set();
        }

        public void Dispose()
        {
            var wasDisposed = _isDisposed;

            _isDisposed = true;

            if (!wasDisposed)
            {
                _waitHandle.Set();

                _runHandle.WaitOne();

                _waitHandle.Dispose();
                //_worker.Dispose();
            }
        }

        private class DeferredTask
        {
            public string Id { get; set; }
            public bool CanBeRun
            {
                get
                {
                    return (Modified + Delay) < DateTime.UtcNow;
                }
            }

            public Task Run()
            {
                WasExecuted = true;
                return Task.Invoke();
            }

            public bool WasExecuted { get; set; }

            public TimeSpan Delay { get; set; }
            public DateTime Created { get; set; } = DateTime.UtcNow;
            public DateTime Modified { get; set; } = DateTime.UtcNow;
            public Func<Task> Task { get; set; }
        }
    }
}