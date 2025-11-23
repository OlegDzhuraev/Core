using UnityEngine;

namespace InsaneOne.Core.Components
{
	[DisallowMultipleComponent]
	public sealed class PhysicsImpulse2d : MonoBehaviour
	{
		[SerializeField] bool applyOnStart = true;
		[SerializeField] ForceMode2D type = ForceMode2D.Impulse;
		[Tooltip("Use for force impulse")]
		[SerializeField] Vector2 impulse;
		[Tooltip("Use for rotation impulse")]
		[SerializeField] float torqueImpulse;
		
		public bool ApplyOnStart { get => applyOnStart; set => applyOnStart = value; }
		public ForceMode2D Type { get => type; set => type = value; }
		public Vector2 Impulse { get => impulse; set => impulse = value; }
		public float TorqueImpulse { get => torqueImpulse; set => torqueImpulse = value; }

		Rigidbody2D r2d;
		
		void Awake() => r2d = GetComponent<Rigidbody2D>();

		void Start()
		{
			if (applyOnStart)
				Apply();
		}

		public void Apply()
		{
			r2d.AddForce(impulse, type);
			r2d.AddTorque(torqueImpulse * Mathf.Deg2Rad * r2d.inertia, type);
		}
	}
}