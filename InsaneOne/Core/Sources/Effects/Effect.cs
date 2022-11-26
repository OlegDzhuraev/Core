using UnityEngine;

namespace InsaneOne.Core
{
    public static class Effect
    {
        const string effectsParentName = "[Effects]";
        
        static Transform effectsParent;
        
        /// <summary> Create VFX from gameobject with specified position and rotation. You can set parent and auto-destroy time. Use -1 to infinite lifetime. </summary>
        public static GameObject Create(GameObject vfxPrefab, Vector3 position,
            Quaternion rotation = default, Transform parent = null, float destructionDelay = 15f)
        {
            var vfx = GameObject.Instantiate(vfxPrefab, position, rotation);
            
            vfx.transform.SetParent(parent != null ? parent : GetEffectsParent());
            
            if (destructionDelay <= -1)
            {
                var delayedDestruction = vfx.GetComponent<DelayedDestruction>();
                
                if (delayedDestruction)
                    GameObject.Destroy(delayedDestruction);
            }
            else
            {
                vfx.DelayedDestroy(destructionDelay);
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
