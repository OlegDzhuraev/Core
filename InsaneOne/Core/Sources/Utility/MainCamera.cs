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
                if (!cachedCamera)
                    cachedCamera = Camera.main;

                return cachedCamera;
            }
            set => cachedCamera = value;
        }	
		
        static Camera cachedCamera;
    }
}