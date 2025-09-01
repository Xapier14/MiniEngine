using System;

namespace MiniEngine.Utility.Bits
{
    public class Timer(long intervalMs)
    {
        private long _lastTimeStamp = -1;

        public bool UpdateAndCheckElapsed()
        {
            var millis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (_lastTimeStamp < 0)
            {
                _lastTimeStamp = millis;
                return false;
            }
            var duration = millis - _lastTimeStamp;
            if (duration < intervalMs)
                return false;
            _lastTimeStamp = millis;
            return true;
        }
    }
}
