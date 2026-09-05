using System;
using UnityEngine;

namespace InsaneOne.Core
{
    [CreateAssetMenu(menuName = "InsaneOne/Core Data")]
    public sealed class CoreData : ScriptableObject
    {
        const string ResourcesPath = "InsaneOne/CoreData";

        static CoreData instance;

        [Header("Features settings")]
#if INSANE_TEAMS_EXTENSION
        public Teams.TeamsSettings TeamsSettings;
#endif
        
        [Tooltip("Place here prefab, which will be used as UI fader.")]
        public GameObject UiFaderTpl;


        [Header("Debugging")]
        [Tooltip("If you don't want to see plugin logs, enable this.")]
        public bool SuppressLogs;

        public static CoreData Load()
        {
            if (!TryLoad(out var result))
                throw new NullReferenceException("Possible, InsaneOne.Core initialization was failed, no CoreData found!");
            
            return result;
        }

        public static bool TryLoad(out CoreData result)
        {
            if (!instance)
                instance = LoadFromResources();

            if (!instance)
            {
                result = default;
                return false;
            }

            result = instance;
            return true;
        }

        static CoreData LoadFromResources() => Resources.Load<CoreData>(ResourcesPath);
    }
}