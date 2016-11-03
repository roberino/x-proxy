using System;
using System.Timers;

namespace XProxy.Core
{
    class AutoInvoker<T> : IDisposable
    {
        private readonly Timer _timer;
        private readonly Action<T> _action;
        private readonly T _state;

        private bool _isDirty;

        public AutoInvoker(Action<T> action, T state, TimeSpan? interval = null)
        {
            _action = action;
            _state = state;

            _timer = new Timer(interval.GetValueOrDefault(TimeSpan.FromSeconds(5)).TotalMilliseconds);

            _timer.Elapsed += OnFire;
        }

        public T State { get { return _state; } }

        public void Trigger()
        {
            _isDirty = true;
            if (_isDirty) _timer.Enabled = true;
        }

        public void Run()
        {
            OnFire();
        }

        private void OnFire(object sender, ElapsedEventArgs e)
        {
            OnFire();
        }

        private void OnFire()
        {
            _timer.Enabled = false;

            if (_isDirty)
            {
                try
                {
                    _action(_state);
                    _isDirty = false;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
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
    }
}