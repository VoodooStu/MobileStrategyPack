using System;
using System.Collections.Generic;
using static Voodoo.Sauce.Internal.Ads.AdUnits;

namespace Voodoo.Sauce.Internal.Ads
{
    public class AdTimer
    {
        private DateTime _startLoadingTime;
        private TimeSpan _loadingTime;

        public AdTimer()
        {
        }

        public double TotalMilliseconds => GetTime().TotalMilliseconds;
        public double TotalSeconds => GetTime().TotalSeconds;

        public void Start()
        {
            _startLoadingTime = DateTime.Now;
        }

        public void Stop()
        {
            _loadingTime = DateTime.Now - _startLoadingTime;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private TimeSpan GetTime()
        {
            var time = _loadingTime;
            if (time.TotalMilliseconds >= 0)
                return time;
            return TimeSpan.Zero;
        }
    }
    
    public class AdTimers
    {
        private Dictionary<string, DateTime> _startLoadingTimes;
        private Dictionary<string, TimeSpan> _loadingTimes;

        public AdTimers()
        {
            _startLoadingTimes = new Dictionary<string, DateTime>();
            _loadingTimes = new Dictionary<string, TimeSpan>();
        }
        
        public void SetStartLoadingTime(AdUnit adUnit, bool isPrimary = true)
        {
            _startLoadingTimes[adUnit.ToString() + isPrimary] = DateTime.Now;
        }

        public void SetEndLoadingTime(AdUnit adUnit, bool isPrimary = true)
        {
            if (_startLoadingTimes.ContainsKey(adUnit.ToString() + isPrimary))
                _loadingTimes[adUnit.ToString() + isPrimary] = DateTime.Now - _startLoadingTimes[adUnit.ToString() + isPrimary];
        }

        public void SetRestartLoadingTime(AdUnit adUnit, bool isPrimary = true)
        {
            SetEndLoadingTime(adUnit, isPrimary);
            SetStartLoadingTime(adUnit, isPrimary);
        }

        public TimeSpan GetLoadingTime(AdUnit adUnit, bool isPrimary = true)
        {
            if(_loadingTimes.ContainsKey(adUnit.ToString() + isPrimary)) {
                var time = _loadingTimes[adUnit.ToString() + isPrimary];
                if (time.TotalMilliseconds >= 0)
                    return time;
            }

            return TimeSpan.Zero;
        }
    }
}