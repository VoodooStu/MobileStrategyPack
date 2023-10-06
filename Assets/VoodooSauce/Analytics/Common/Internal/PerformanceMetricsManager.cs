using System;
using System.Collections;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.DeviceInfo;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class PerformanceMetricsManager
    {
        private static bool _applicationUnpaused;

        public static void Initialize(float period)
        {
            VoodooSauceBehaviour.InvokeCoroutine(new PerformanceMetricsManager().PerformanceMetricsCoroutine(period));
        }

        internal static void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) {
                _applicationUnpaused = true;
            }
        }

        private IEnumerator PerformanceMetricsCoroutine(float period)
        {
            float timer = 0f;

            float minMs = 99f;
            float maxMs = 0f;
            float numFrames = 0f;
            int badFrames = 0;
            int terribleFrames = 0;

            const float badFrameThreshold = 1f / 60f * 3f; // 20 fps
            const float terribleFrameThreshold = 1f / 60f * 10f; // 6 fps -

            while (true)
            {
                yield return null;
                float frameLength = Time.unscaledDeltaTime;
                if (_applicationUnpaused) {
                    _applicationUnpaused = false;
                    //skip this frame from the FPS calculation
                    continue;
                }
                timer += frameLength;
                
                minMs = Math.Min(minMs, frameLength);
                maxMs = Math.Max(maxMs, frameLength);
                numFrames++;
                if (frameLength > badFrameThreshold) badFrames++;
                if (frameLength > terribleFrameThreshold) terribleFrames++;

                if (timer >= period)
                {
                    SendPerformanceMetricsEvent(minMs, maxMs, numFrames/period, badFrames, terribleFrames, GC.GetTotalMemory(false));
                    timer = 0f;
                    numFrames = 0f;
                    minMs = 99f;
                    maxMs = 0f;
                    badFrames = 0;
                    terribleFrames = 0;
                }
            }
        }

        private void SendPerformanceMetricsEvent(float minMs, float maxMs, float aveFPS, int badFrames, int terribleFrames, long memoryUsed)
        {
            PerformanceMetricsAnalyticsInfo info;
            info.BatteryLevel = SystemInfo.batteryLevel;
            info.Volume = Audio.GetVolume();
            info.Fps.Min = 1f / maxMs;
            info.Fps.Max = 1f / minMs;
            info.Fps.Average = aveFPS;
            info.MemoryUsage.Min = memoryUsed;
            info.MemoryUsage.Max = memoryUsed;
            info.MemoryUsage.Average = memoryUsed;
            info.AverageMemoryUsagePercentage = Math.Abs(memoryUsed/(1048576.0*SystemInfo.systemMemorySize));
            info.BadFrames = badFrames;
            info.TerribleFrames = terribleFrames;
            AnalyticsManager.TrackPerformanceMetrics(info);
        }
    }
}