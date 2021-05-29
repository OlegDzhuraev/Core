using UnityEngine;

namespace InsaneOne.Core
{
    public static class MainCamera
    {
        /// <summary> Returns Main Camera, cached in a variable on any scene. </summary>
        public static Camera Cached 
        {
            get
            {
                if (!cachedCamera)
                    cachedCamera = Camera.main;

                return cachedCamera;
            }
        }	
		
        static Camera cachedCamera;
    }
}