using UnityEngine;

namespace InsaneOne.Core.Components
{
	public enum LookType
	{
		None,
		Mouse,
		Point,
		TransformTarget
	}

	public sealed class LookAt2d : MonoBehaviour
	{
		[SerializeField] LookType lookType;
		[SerializeField] bool isInstant = true;
		[Tooltip("Used only if instant value is false.")]
		[SerializeField] float rotationSpeed = 3f;
		[SerializeField] Transform target;
		[SerializeField] Vector3 point;

		public Transform Target { get => target; set => target = value; }
		public Vector3 Point { get => point; set => point = value; }
		public bool IsInstant { get => isInstant; set => isInstant = value; }
		public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
		public LookType LookType { get => lookType; set => lookType = value; }
		
		void Update()
		{
			var lookPosition = LookType switch
			{
				LookType.Mouse => InputExtensions.GetMouseWorldPos2D(),
				LookType.TransformTarget when Target => Target.position,
				LookType.Point => Point,
				_ => transform.position
			};

			if (IsInstant)
				transform.LookAt2D(lookPosition);
			else 
				transform.RotateTo2D(lookPosition, RotationSpeed);
		}
	}
}