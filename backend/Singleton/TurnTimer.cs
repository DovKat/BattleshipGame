using System;
using System.Runtime.Remoting;
using System.Threading;

namespace backend.Singleton
{
    public class TurnTimer
    {
        private static TurnTimer _instance;
        private static readonly object threadLock = new object();
        private Timer _timer;
        private int _timeLeft;
        private Action _onTimerUp;
        private TurnTimer() 
        {

        }

        public static TurnTimer Instance 
        {
            get
            {
                lock(threadLock)
                {
                    if(_instance == null)
                    {
                        _instance = new TurnTimer();
                    }
                    return _instance;
                }
            }
        }

        public void StartTimer(int durationInSeconds, Action onTimeUp)
        {
            _timeLeft = durationInSeconds;
            _onTimerUp = onTimeUp;
            _timer = new Timer(TimerTick, null, 1000, 1000);
        }

        public void TimerTick(object state)
        {
            _timeLeft--;

            if(_timeLeft <= 0)
            {
                _timer.Dispose();
                _onTimerUp?.Invoke();
            }
        }

        public void StopTimer()
        {
            _timer?.Dispose();
        }
    }
}
