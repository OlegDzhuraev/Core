using UnityEngine;

namespace InsaneOne.Core
{
	public static class TransformExtensions
	{
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

		/// <summary> Rotates a 2D object in cursor direction, make it looking to the position. </summary>
		public static void LookAtMouse2D(this Transform transform)
		{
			var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
			var mousePosOnScreen = MainCamera.Cached.ScreenToWorldPoint(mousePos);
			mousePosOnScreen.z = transform.position.z;

			transform.LookAt2D(mousePosOnScreen);
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

			transform.rotation =
				Quaternion.RotateTowards(transform.rotation, rotation, angularSpeed * Time.smoothDeltaTime);
		}

		/// <summary> Returns look to the position in 2D. </summary>
		public static Quaternion GetLook2D(this Transform transform, Vector2 position)
		{
			var direction = ((Vector3) position - transform.position).normalized;
			return Quaternion.LookRotation(Vector3.forward, direction);
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
			var targetDirection = (target.position - self.position).normalized;
            
			return Vector2.Angle(self.up, targetDirection) < threshold;
		}
	}
}