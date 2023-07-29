using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Utility
{
    // TODO InsaneOne: Allow to pause custom time scales?
    public static class PauseUtility
    {
        static readonly List<IPauseAffector> affectors = new List<IPauseAffector>();
        
        /// <summary> Pause by some affector (any object, which can pause app). Them stacks to allow make game be paused, when one of them called "Unpause", and other still pausing game. </summary>
        public static void Pause(IPauseAffector pauseBy)
        {
            affectors.Add(pauseBy);

            Time.timeScale = 0;
        }

        /// <summary> Unpauses by specific affector. If there others affectors, app will continue by paused. </summary>
        public static void Unpause(IPauseAffector unpauseBy)
        {
            affectors.Remove(unpauseBy);

            if (affectors.Count == 0)
                Time.timeScale = 1f;
        }

        public static bool IsPaused() => affectors.Count > 0;

        /// <summary> Call this on app start or end one time. It will reset all pause affectors and timescale. </summary>
        public static void Reset()
        {
            affectors.Clear();
            Time.timeScale = 1f;
        }
    }
}