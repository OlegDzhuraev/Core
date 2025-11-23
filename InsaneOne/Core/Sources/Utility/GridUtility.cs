using System.Runtime.CompilerServices;
using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public static class GridUtility
	{
		public static Vector3 SnapToGrid(Vector3 position, float gridSize = 1f)
		{
			Debug.Assert(gridSize != 0);

			var x = SnapValueTo(position.x, gridSize);
			var y = SnapValueTo(position.y, gridSize);
			var z = SnapValueTo(position.z, gridSize);
			
			return new Vector3(x, y, z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float SnapValueTo(float value, float step) => Mathf.Round(value / step) * step;
	}
}