using UnityEngine;

namespace InsaneOne.Core
{
    public static class MainCamera
    {
        /// <summary> Returns Main Camera, cached in a variable. Works on any scene. You can set custom camera. </summary>
        public static Camera Cached 
        {
            get
            {
                if (!cachedCamera || !cachedCamera.isActiveAndEnabled) // also refresh when the cached camera was disabled or replaced by another main camera
                    cachedCamera = Camera.main;

                return cachedCamera;
            }
            set => cachedCamera = value;
        }	
		
        static Camera cachedCamera;
    }
}