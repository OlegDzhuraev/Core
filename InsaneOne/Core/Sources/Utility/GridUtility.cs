using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public static class GridUtility
	{
		public static Vector3 SnapToGrid(Vector3 position, float gridSize = 1f)
		{
			Debug.Assert(gridSize != 0);

			var x = Mathf.Round(position.x / gridSize) * gridSize;
			var y = Mathf.Round(position.y / gridSize) * gridSize;
			var z = Mathf.Round(position.z / gridSize) * gridSize;

			return new Vector3(x, y, z);
		}
	}
}