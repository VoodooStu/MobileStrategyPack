using System;
using System.Collections;
using UnityEngine;
using Voodoo.Sauce.Internal;

public class ExponentialBackoff : MonoBehaviour
{
    private const string TAG = "ExponentialBackoff";

    public class ExpBackoff
    {
        public float[] Delays;
        public Action Callback;
        public string Name;

        private Coroutine _coroutine;
        private int _currentDelayIndex;

        public ExpBackoff() => _currentDelayIndex = 0;

        public void Start()
        {
            if (_coroutine != null) return;

            float delay = Delays[_currentDelayIndex];

            if (_currentDelayIndex < Delays.Length - 1)
                _currentDelayIndex++;

            if (delay > 0) {
                _coroutine = _instance.StartCoroutine(BackoffCoroutine(delay, () => {
                    Stop();
                    Callback();
                }, Name));
            } else {
                Callback();
            }
        }

        public void Reset()
        {
            _currentDelayIndex = 0;
            Stop();
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        private void Stop()
        {
            if (_coroutine != null) {
                _instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }

    private static ExponentialBackoff _instance;

    private void Awake()
    {
        _instance = this;
    }

    public static ExpBackoff CreateExpBackoff(Action callback, string name, float[] delays = null)
    {
        if (delays == null || delays.Length == 0) {
            delays = new[] {0f, 1f, 2f, 4f, 8f, 15f, 30f, 60f};
        }

        var backoff = new ExpBackoff {Delays = delays, Callback = callback, Name = name};
        return backoff;
    }

    private static IEnumerator BackoffCoroutine(float delay, Action callback, string name)
    {
        VoodooLog.LogDebug(Module.ADS, TAG, $"{name} will wait: {delay} seconds before callback");
        yield return new WaitForSecondsRealtime(delay);
        VoodooLog.LogDebug(Module.ADS, TAG, $"{name} calling callback...");
        callback();
    }
}