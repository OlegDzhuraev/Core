using System;
using UnityEngine;

namespace InsaneOne.Core.Components
{
	[Flags] 
	public enum ClampAxes { X = 1, Y = 2, Z = 4 }

	[DefaultExecutionOrder(100)]
	public class ClampPosition : MonoBehaviour
	{
		[SerializeField] ClampAxes clampAxes = ClampAxes.X;
		[Tooltip("Should be clamp applied from transform coordinates instead of world zero?")]
		[SerializeField] bool fromStart;
		[SerializeField] Vector2 minMaxX = new (0, 1024);
		[SerializeField] Vector2 minMaxY = new (0, 1024);
		[SerializeField] Vector2 minMaxZ = new (0, 1024);
		[SerializeField] bool useLateUpdate;

		Vector3 startOffset;

		void Start() => startOffset = transform.position;

		void Update()
		{
			if (!useLateUpdate)
				Process();
		}

		void LateUpdate()
		{
			if (useLateUpdate)
				Process();
		}

		void Process()
		{
			var position = transform.position;
			
			if (clampAxes.HasFlag(ClampAxes.X))
				position.x = Clamp(position.x, minMaxX, startOffset.x);
			
			if (clampAxes.HasFlag(ClampAxes.Y))
				position.y = Clamp(position.y, minMaxY, startOffset.y);
			
			if (clampAxes.HasFlag(ClampAxes.Z))
				position.z = Clamp(position.z, minMaxZ, startOffset.z);

			transform.position = position;
		}

		float Clamp(float valueCoord, Vector2 minMax, float offsetCoord)
		{
			var realOffset = fromStart ? offsetCoord : 0;
			
			return Mathf.Clamp(valueCoord, minMax.x + realOffset, minMax.y + realOffset);
		}
		
		public void SetAxes(ClampAxes axes) => clampAxes = axes;

		public void SetClampValue(ClampAxes axes, Vector2 value)
		{
			if (axes.HasFlag(ClampAxes.X))
				minMaxX = value;
			if (axes.HasFlag(ClampAxes.Y))
				minMaxY = value;
			if (axes.HasFlag(ClampAxes.Z))
				minMaxZ = value;
		}
	}
}