using UnityEngine;

namespace InsaneOne.Core
{
	public static class InputExtensions
	{
		static bool loggedNoCameraWarning;

		public static Vector3 GetMouseWorldPos2D()
		{
			var camera = MainCamera.Cached;

			if (camera == null)
			{
				if (!loggedNoCameraWarning)
				{
					Debug.LogWarning($"[{nameof(MainCamera)}] No main camera found - can't get mouse world position!");
					loggedNoCameraWarning = true;
				}

				return default;
			}

			var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
			return camera.ScreenToWorldPoint(mousePos);
		}
	}
}