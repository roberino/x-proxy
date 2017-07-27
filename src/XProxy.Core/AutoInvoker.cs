using System;
using System.Threading;

namespace XProxy.Core
{
    class AutoInvoker<T> : IDisposable
    {
        private readonly Timer _timer;
        private readonly Action<T> _action;
        private readonly T _state;
        private readonly TimeSpan _interval;
        private readonly Func<T, bool> _triggerCheck;

        private bool _isDirty;
        private bool _isPaused;

        public AutoInvoker(Action<T> action, T state, Func<T, bool> triggerCheck = null, TimeSpan? interval = null)
        {
            _action = action;
            _state = state;
            _interval = interval.GetValueOrDefault(TimeSpan.FromMilliseconds(200));

            _timer = new Timer(c =>
            {
                OnFire();   
            }, true, TimeSpan.Zero, _interval);
            
            _triggerCheck = triggerCheck == null ?
                (_ => _isDirty) : (Func<T, bool>)(s => _isDirty || triggerCheck(s));
        }

        public IDisposable Pause()
        {
            _timer.Change(Timeout.Infinite, 0);
            _isPaused = true;

            return new PauseState(() =>
            {
                _isPaused = false;
                _timer.Change(TimeSpan.Zero, _interval);
            });
        }

        public T State { get { return _state; } }

        public void Trigger()
        {
            _isDirty = true;
            if (!_isPaused)
                _timer.Change(TimeSpan.Zero, _interval);
        }

        public void Run()
        {
            OnFire();
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, 0);

            if (_isDirty)
            {
                OnFire();
            }

            _timer.Dispose();
        }

        private void OnFire()
        {
            if (_isPaused) return;

            _timer.Change(Timeout.Infinite, 0);

            if (_triggerCheck(State))
            {
                try
                {
                    _action(_state);
                    _isDirty = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (_triggerCheck(State))
                {
                    _timer.Change(TimeSpan.Zero, _interval);
                }
            }
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