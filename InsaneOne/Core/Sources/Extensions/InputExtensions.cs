using UnityEngine;

namespace InsaneOne.Core
{
	public static class InputExtensions
	{
		public static Vector3 GetMouseWorldPos2D()
		{
			var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
			return MainCamera.Cached.ScreenToWorldPoint(mousePos);
		}
	}
}