using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.LoadingTime
{
    /*
     * Util class used to measure the loading time of an event.
     * 
     * The loading time of an event is defined by:
     * - a start timestamp, when the event starts 
     * - an end timestamp when the event ends
     * - a pause time, between the start and the end of an event an event can be paused
     *
     * The duration of the loading time of an event is define by the elapsed time between its start and its end
     * minus its total pause time.
     *
     * àn event can be paused many times before it is stopped.
     * An event can not be paused once it is finished.
     * An event can be stopped only once.
     */
    internal class LoadingTimer
    {
        private DateTimeOffset _start;
        private DateTimeOffset? _end;
        private DateTimeOffset? _startPause;
        private long _pauseInMilliseconds;

#region Constructors
        
        internal LoadingTimer() => _start = new DateTimeOffset(DateTime.UtcNow);

        internal LoadingTimer(DateTimeOffset start) => _start = start;

#endregion

#region Life cycle

        internal void Pause()  {
            if (IsStopped()) {
                _startPause = null;
                return;
            }

            if (_startPause != null) {
                return;
            }
            
            _startPause = DateTimeOffset.Now;   
        }

        internal void Unpause() {
            if (IsStopped()) {
                _startPause = null;
                return;
            }
            
            DateTimeOffset endPause = DateTimeOffset.Now;
            DateTimeOffset startPause = _startPause ?? endPause;
            
            _pauseInMilliseconds += (endPause.ToUnixTimeMilliseconds() - startPause.ToUnixTimeMilliseconds());
            _startPause = null;
        }

        internal void SetEnd(DateTimeOffset end) {
            // Ensure that the timer is ended only once.
            if (IsStopped()) {
                return;
            }
            
            _end = end;
        }

        internal long Stop() {
            SetEnd(DateTimeOffset.Now);
            return GetDuration();
        }

        internal bool IsStopped() => _end != null;

        internal long GetStartTimestamp() => _start.ToUnixTimeMilliseconds();
        
        internal long GetEndTimestamp() => _end?.ToUnixTimeMilliseconds() ?? 0;
        
        internal long GetDuration() => IsStopped() ? GetEndTimestamp() - GetStartTimestamp() - _pauseInMilliseconds : -1;

#endregion
    }
}