using UnityEngine;

namespace InsaneOne.Core
{
    [CreateAssetMenu(menuName = "InsaneOne/Core Data")]
    public sealed class CoreData : ScriptableObject
    {
        public GameObject UiFaderTpl;
        [Tooltip("If you don't want to see plugin logs, set this.")]
        public bool SuppressLogs;
    }
}