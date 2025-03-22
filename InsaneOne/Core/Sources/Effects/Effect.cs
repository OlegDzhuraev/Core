using UnityEngine;
#if PERSEIDS_POOLING
using InsaneOne.PerseidsPooling;
using InsaneOne.PerseidsPooling.Utils;
#endif

namespace InsaneOne.Core
{
    /// <summary> VFX spawner class. Will use pooling system, if Perseids Pooling is added to the project. </summary>
    public static class Effect
    {
        const float DefaultDestructionDelay = 15f;

        const string effectsParentName = "[Effects]";
        
        static Transform effectsParent;

        /// <summary> Create VFX from GameObject prefab with specified position and rotation. You can set parent and auto-destroy time. Use -1 to infinite lifetime. </summary>
        public static bool TryCreate(GameObject vfxPrefab, Vector3 position,
            Quaternion rotation = default, Transform parent = null, float destructionDelay = DefaultDestructionDelay)
        {
            return TryCreate(vfxPrefab, position, out _, rotation, parent, destructionDelay);
        }

        /// <summary> Create VFX from GameObject prefab with specified position and rotation. You can set parent and auto-destroy time. Use -1 to infinite lifetime. </summary>
        public static bool TryCreate(GameObject vfxPrefab, Vector3 position, out GameObject effect,
            Quaternion rotation = default, Transform parent = null, float destructionDelay = DefaultDestructionDelay)
        {
            if (!vfxPrefab)
            {
                effect = default;
                return false;
            }

            effect = Create(vfxPrefab, position, rotation, parent, destructionDelay);
            return true;
        }

        /// <summary> Create VFX from GameObject prefab with specified position and rotation. You can set parent and auto-destroy time. Use -1 to infinite lifetime. </summary>
        public static GameObject Create(GameObject vfxPrefab, Vector3 position,
            Quaternion rotation = default, Transform parent = null, float destructionDelay = DefaultDestructionDelay)
        {
#if PERSEIDS_POOLING
            var vfx = Pool.Spawn(vfxPrefab);
            vfx.transform.position = position;
            vfx.transform.rotation = rotation;
#else 
            var vfx = GameObject.Instantiate(vfxPrefab, position, rotation);
#endif
            
            vfx.transform.SetParent(parent != null ? parent : GetEffectsParent());

#if PERSEIDS_POOLING
            var delayedDestruction = vfx.GetComponent<DelayedPoolDestruction>();
#else
            var delayedDestruction = vfx.GetComponent<DelayedDestruction>();
#endif

            if (destructionDelay <= -1)
            {
                if (delayedDestruction)
                    GameObject.Destroy(delayedDestruction);
            }
            else
            {
                if (!Mathf.Approximately(destructionDelay, DefaultDestructionDelay) || delayedDestruction == null) // will be applied only if there are no custom delay attached to prefab, or if custom delay value passed to this method
                    vfx.DelayedDestroy(destructionDelay, true);
            }

            return vfx;
        }

        static Transform GetEffectsParent()
        {
            if (!effectsParent)
            {
                var effectsParentGo = GameObject.Find(effectsParentName);

                if (!effectsParentGo)
                    effectsParentGo = new GameObject(effectsParentName);
                    
                effectsParent = effectsParentGo.transform;
            }

            return effectsParent;
        }
    }
}
