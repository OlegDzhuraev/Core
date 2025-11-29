using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Utility
{
    public static class PauseUtility
    {
        const float DefaultTimeScale = 1f;

        /// <summary> After unpause, timescale will be restored to this value. By default, it is 1. </summary>
        public static float TimeScale = DefaultTimeScale;

        static readonly HashSet<IPauseSource> sources = new ();

        /// <summary> Pause by some source (any object, which can pause app). Allowed sources stacking to allow make game be paused, when one of them called "Unpause", and other still pausing game. </summary>
        public static void Pause(IPauseSource pauseBy)
        {
            if (!sources.Add(pauseBy))
            {
                CoreUnityLogger.I.Log($"Pause source [{pauseBy}] was already added to the pause sources! Ignored pause request.", LogLevel.Warning);
                return;
            }

            Time.timeScale = 0;
        }

        /// <summary> Unpauses by specific source. If there is any other source, app will continue by paused. </summary>
        public static void Unpause(IPauseSource unpauseBy)
        {
            if (!sources.Remove(unpauseBy))
            {
                CoreUnityLogger.I.Log($"Pause source [{unpauseBy}] wasn't added to the pause sources! Ignored unpause request.", LogLevel.Warning);
                return;
            }

            if (sources.Count == 0)
                Time.timeScale = TimeScale;
        }

        public static bool IsPaused() => sources.Count > 0;

        /// <summary> Call this every game state/scope change! It will reset all pause sources and timescale. </summary>
        public static void Reset()
        {
            sources.Clear();
            TimeScale = DefaultTimeScale;
            Time.timeScale = TimeScale;
        }
    }
}