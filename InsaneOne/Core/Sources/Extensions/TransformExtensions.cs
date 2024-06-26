using UnityEngine;

namespace InsaneOne.Core
{
	public static class TransformExtensions
	{
		public static bool RightAxisIsForwardInTwoD;
		
		public static void SetPositionX(this Transform transform, float value)
		{
			var vector3 = transform.position;
			vector3 = new Vector3(value, vector3.y, vector3.z);
			transform.position = vector3;
		}

		public static void SetPositionY(this Transform transform, float value)
		{
			var vector3 = transform.position;
			vector3 = new Vector3(vector3.x, value, vector3.z);
			transform.position = vector3;
		}

		public static void SetPositionZ(this Transform transform, float value)
		{
			var vector3 = transform.position;
			vector3 = new Vector3(vector3.x, vector3.y, value);
			transform.position = vector3;
		}

		public static void Set2DRotation(this Transform transform, float value) => transform.localEulerAngles = new Vector3(0, 0, value);

		public static float Get2DRotation(this Transform transform) => transform.localEulerAngles.z;

		/// <summary> Rotates a 2D object in cursor direction, make it looking to the position. </summary>
		public static void LookAtMouse2D(this Transform transform)
		{
			var mouseWorldPos = InputExtensions.GetMouseWorldPos2D();
			mouseWorldPos.z = transform.position.z;
			transform.LookAt2D(mouseWorldPos);
		}
		
		/// <summary> Rotates a 2D object, make it looking to the position. </summary>
		public static void LookAt2D(this Transform transform, Vector3 position)
		{
			transform.rotation = transform.GetLook2D(position);
		}

		/// <summary> Rotates a 2D object to the position with input angular speed. </summary>
		public static void RotateTo2D(this Transform transform, Vector2 position, float angularSpeed)
		{
			var rotation = transform.GetLook2D(position);
			var delta = angularSpeed * Time.smoothDeltaTime;
			
			transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, delta);
		}

		/// <summary> Returns look to the position in 2D. </summary>
		public static Quaternion GetLook2D(this Transform transform, Vector2 position)
		{
			return GetLook2D(transform.position, position);
		}
		
		/// <summary> Returns look to the position in 2D. </summary>
		public static Quaternion GetLook2D(Vector2 positionA, Vector2 positionB)
		{
			var direction = (positionB - positionA).normalized;
			var result = Quaternion.LookRotation(Vector3.forward, direction);
			
			if (RightAxisIsForwardInTwoD)
				result *= Quaternion.Euler(new Vector3(0, 0, 90));
			
			return result;
		}

		/// <summary> Returns look to the position in 2D. </summary>
		public static float GetAngleLook2D(this Transform transform, Vector2 position)
		{
			return GetAngleLook2D(transform.position, position);
		}

		/// <summary> Returns look to the position in 2D. </summary>
		public static float GetAngleLook2D(Vector2 positionA, Vector2 positionB)
		{
			var direction = (positionB - positionA).normalized;
			var result = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			if (!RightAxisIsForwardInTwoD)
				result -= 90;

			return result;
		}

		/// <summary> Returns direction from 2D angle. </summary>
		public static Vector2 GetDirectionFromAngle2D(float angle)
		{
			if (!RightAxisIsForwardInTwoD)
				angle += 90;

			var angleInRadians = angle * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
		}

		/// <summary> Returns is transfrom looking on a target transform. </summary>
		/// <param name="threshold">Angle value of tolerance. </param>
		public static bool IsLookingAt(this Transform self, Transform target, float threshold = 3f, bool ignoreY = true)
		{
			Vector3 targetDirection;

			if (ignoreY)
			{
				var targetPositionSameY = target.position;
				targetPositionSameY.y = self.position.y;

				targetDirection = (targetPositionSameY - self.position).normalized;
			}
			else
			{
				targetDirection = (target.position - self.position).normalized;
			}

			return Vector3.Angle(self.forward, targetDirection) < threshold;
		}
		
		/// <summary> Returns is transfrom looking on a target transform. </summary>
		/// <param name="threshold">Angle value of tolerance. </param>
		public static bool IsLookingAt2D(this Transform self, Transform target, float threshold = 3f)
		{
			var forward = RightAxisIsForwardInTwoD ? self.right : self.up;
			return IsLookingAt2D(self.position, forward, target.position, threshold);
		}
		
		/// <summary> Returns is position a looking on a position b. Requires forawrd vector direction. For 2d it can be transform.up or transform.right.</summary>
		/// <param name="threshold">Angle value of tolerance. </param>
		public static bool IsLookingAt2D(Vector2 positionA, Vector2 forwardA, Vector2 positionB, float threshold = 3f)
		{
			var targetDirection = (positionB - positionA).normalized;
			return Vector2.Angle(forwardA, targetDirection) < threshold;
		}
		
		/// <summary> Find child by name with recursive search in all children objects. </summary>
		public static Transform RecursiveFind(this Transform parent, string childName)
		{
			foreach (Transform child in parent)
			{
				if (child.name == childName)
					return child;

				var found = RecursiveFind(child, childName);
				if (found != null)
					return found;
			}

			return null;
		}
	}
}