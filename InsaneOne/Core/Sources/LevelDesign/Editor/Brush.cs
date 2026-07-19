using UnityEngine;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Placement settings asset for the Object Placer tool: surface constraints and rotation/scale/position randomization. </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Level Design/Brush")]
	public class Brush : ScriptableObject
	{
		[Header("Surface")]
		[Tooltip("Prefabs won't be placed on surfaces steeper than this angle from the world up vector.")]
		[Range(0f, 90f)]
		public float MaxSlopeAngle = 90f;

		[Tooltip("Rotates the placed prefab's up vector to match the surface normal.")]
		public bool AlignToNormal;

		[Header("Rotation")]
		public bool RandomizeRotation;
		[Range(0f, 180f)]
		public float MaxRotationAngle = 45f;

		[Header("Scale")]
		public bool RandomizeScale;
		public Vector2 ScaleRange = new (0.8f, 1.2f);

		[Header("Position")]
		public bool RandomizePosition;
		[Range(0f, 5f)]
		public float MaxPositionOffset = 0.5f;

		[Header("Grid")]
		public bool SnapToGrid;
		[Range(0.01f, 10f)]
		public float GridSize = 1f;
	}
}
