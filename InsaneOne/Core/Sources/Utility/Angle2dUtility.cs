using UnityEngine;

namespace InsaneOne.Core
{
	public static class Angle2dUtility
	{
		public static void RotateToPositionOnCircle(this Transform transform, float angleOffset, int id, int count)
		{
			var angle = GetAngleByPositionOnCircle(angleOffset, id, count);
			transform.Rotate(0, 0, angle);
		}

		public static float GetAngleByPositionOnCircle(float angleOffset, int id, int count)
		{
			var halfAngle = count / 2f * angleOffset;
			return Mathf.Lerp(-halfAngle, halfAngle, id / (float) (count - 1));
		}
	}
}