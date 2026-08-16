using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public static class Angle2dUtility
	{
		/// <summary>Rotates the transform locally around Z by the angle computed for its position on the circle.</summary>
		public static void RotateToPositionOnCircle(this Transform transform, float angleOffset, int id, int count)
		{
			var angle = GetAngleByPositionOnCircle(angleOffset, id, count);
			transform.Rotate(0, 0, angle);
		}

		/// <summary>Returns the angle of the id-th of count positions evenly spread angleOffset degrees apart, centered on 0.</summary>
		public static float GetAngleByPositionOnCircle(float angleOffset, int id, int count)
		{
			if (count <= 1)
				return 0f;

			var halfAngle = (count - 1) / 2f * angleOffset;
			return Mathf.Lerp(-halfAngle, halfAngle, id / (float) (count - 1));
		}
	}
}