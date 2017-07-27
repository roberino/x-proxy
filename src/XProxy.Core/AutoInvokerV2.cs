using System;
using System.Threading;

namespace XProxy.Core
{
    class AutoInvokerV2<T> : IDisposable
    {
        private readonly Action<T> _action;
        private readonly T _state;
        private readonly Func<T, bool> _triggerCheck;
        private readonly Thread _worker;

        private bool _isDirty;
        private bool _isPaused;
        private bool _isDisposed;

        public AutoInvokerV2(Action<T> action, T state, Func<T, bool> triggerCheck = null, TimeSpan? interval = null)
        {
            _action = action;
            _state = state;

            _triggerCheck = triggerCheck == null ?
                (_ => _isDirty) : (Func<T, bool>)(s => _isDirty || triggerCheck(s));

            _worker = new Thread(Execute)
            {
                IsBackground = true,
                //Priority = ThreadPriority.Lowest
            };

            _worker.Start();
        }

        public IDisposable Pause()
        {
            _isPaused = true;

            return new PauseState(() =>
            {
                _isPaused = false;
            });
        }

        public T State { get { return _state; } }

        public void Trigger()
        {
            _isDirty = true;
        }

        public void Run()
        {
            OnFire();
        }

        public void Dispose()
        {
            _isDisposed = true;

            if (_isDirty)
            {
                OnFire();
            }
        }

        private void Execute()
        {
            while (!_isDisposed)
            {
                if (!OnFire())
                {
                    Thread.Sleep(50);
                }
            }
        }

        private bool OnFire()
        {
            if (_isPaused) return false;

            if (_triggerCheck(State))
            {
                try
                {
                    _action(_state);
                    _isDirty = false;
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                        Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        private class PauseState : IDisposable
        {
            private readonly Action _onDispose;

            public PauseState(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}