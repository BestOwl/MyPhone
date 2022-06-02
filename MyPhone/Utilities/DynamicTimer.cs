using System;
using System.Collections.Generic;
using System.Timers;

namespace GoodTimeStudio.MyPhone.Utilities
{
    public class DynamicTimer : IDisposable
    {
        private Timer _timer;
        private IEnumerator<DynamicTimerSchedule> _schedules;
        private int _currentTryCount;

        public delegate void DynamicTimerElapsedEventHandler(object? sender, DynamicTimerElapsedEventArgs e);
        public event DynamicTimerElapsedEventHandler? Elapsed;
        public bool Enabled { get => _timer.Enabled; }

        /// <summary>
        /// Initialize a new instance of <see cref="DynamicTimer"/>
        /// </summary>
        /// <param name="schedules">
        /// Collection of <see cref="DynamicTimerSchedule"/> to follow. 
        /// The timer will iterate to next schedule if the current schedule has completed. 
        /// </param>
        public DynamicTimer(IEnumerable<DynamicTimerSchedule> schedules)
        {
            _timer = new Timer();
            _schedules = schedules.GetEnumerator();
            _currentTryCount = 0;
        }

        public void Start()
        {
            if (_schedules.MoveNext())
            {
                DynamicTimerSchedule schedule = _schedules.Current;
                _timer.Interval = schedule.Interval.TotalMilliseconds;
                _currentTryCount = schedule.TryCount;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
            }
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(this, new DynamicTimerElapsedEventArgs(e.SignalTime));
            if (_currentTryCount > 0)
            {
                if (_currentTryCount == 1)
                {
                    if (_schedules.MoveNext())
                    {
                        DynamicTimerSchedule schedule = _schedules.Current;
                        _timer.Interval = schedule.Interval.TotalMilliseconds;
                        _currentTryCount = schedule.TryCount;
                    }
                    else
                    {
                        _timer.Stop();
                    }
                }
                else
                {
                    _currentTryCount--;
                }
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class DynamicTimerElapsedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the date/time when the <see cref="DynamicTimer.Elapsed"/> event was triggered.
        /// </summary>
        public DateTime SignalTime { get; }

        public DynamicTimerElapsedEventArgs(DateTime signalTime)
        {
            SignalTime = signalTime;
        }
    }

    public class DynamicTimerSchedule
    {
        /// <summary>
        /// The number of times to trigger Elapsed event. The timer will stop after the specified number of triggered Elapsed event.
        /// </summary>
        /// <remarks>
        /// Must be greater or equal to zero. If it is zero, the timer will continue indefinitely.
        /// </remarks>
        public int TryCount { get; private set; }

        /// <summary>
        /// The timer interval of the Elapsed event.
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// Create a <see cref="DynamicTimer"/>'s schedule. 
        /// The <see cref="DynamicTimer"/> will fire Elapsed event accoding to this schedule
        /// </summary>
        /// <param name="interval">Speicify the interval of the timer</param>
        /// <param name="tryCount">The number of times to trigger Elapsed</param>
        /// <remarks>
        /// For example, if <paramref name="interval"/> is 1 seconds and <paramref name="tryCount"/> is 2.
        /// If we call <see cref="DynamicTimer.Start"/> at T+0s, the Elapsed event will be trigger at T+1s and T+2s, then the timer will stop.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DynamicTimerSchedule(TimeSpan interval, int tryCount)
        {
            if (tryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tryCount), "must be greater or equal to 0");
            }

            Interval = interval;
            TryCount = tryCount;
        }
    }
}
