using System;
using System.Timers;

namespace XProxy.Core
{
    class AutoInvokerV1<T> : IDisposable
    {
        private readonly Timer _timer;
        private readonly Action<T> _action;
        private readonly T _state;
        private readonly Func<T, bool> _triggerCheck;

        private bool _isDirty;
        private bool _isPaused;

        public AutoInvokerV1(Action<T> action, T state, Func<T, bool> triggerCheck = null, TimeSpan? interval = null)
        {
            _action = action;
            _state = state;

            _timer = new Timer(interval.GetValueOrDefault(TimeSpan.FromMilliseconds(200)).TotalMilliseconds);

            _timer.Elapsed += OnFire;

            _triggerCheck = triggerCheck == null ?
                (_ => _isDirty) : (Func<T, bool>)(s => _isDirty || triggerCheck(s));
        }

        public IDisposable Pause()
        {
            _timer.Enabled = false;
            _isPaused = true;

            return new PauseState(() =>
            {
                _isPaused = false;
                _timer.Enabled = true;
            });
        }

        public T State { get { return _state; } }

        public void Trigger()
        {
            _isDirty = true;
            if (!_isPaused) _timer.Enabled = true;
        }

        public void Run()
        {
            OnFire();
        }

        public void Dispose()
        {
            _timer.Stop();

            if (_isDirty)
            {
                OnFire();
            }

            _timer.Dispose();
        }

        private void OnFire(object sender, ElapsedEventArgs e)
        {
            OnFire();
        }

        private void OnFire()
        {
            if (_isPaused) return;

            _timer.Enabled = false;

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
                    _timer.Enabled = true;
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