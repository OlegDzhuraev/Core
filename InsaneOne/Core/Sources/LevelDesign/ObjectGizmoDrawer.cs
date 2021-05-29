using UnityEngine;

namespace InsaneOne.Core
{
	///<summary> Draws Wire cube with selected color using object collider. Useful to show primary objects in editor.</summary>
	public class ObjectGizmoDrawer : MonoBehaviour
	{
		[SerializeField] Color gizmoColor = Color.red;

		void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = gizmoColor;

			var boxCollider = GetComponent<BoxCollider>();
			var sphereCollider = GetComponent<SphereCollider>();
			var boxCollider2D = GetComponent<BoxCollider2D>();
			var circleCollider = GetComponent<CircleCollider2D>();

			if (boxCollider)
				Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);
			else if (boxCollider2D)
				Gizmos.DrawWireCube(Vector3.zero, boxCollider2D.size);
			else if (sphereCollider)
				Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
			else if (circleCollider)
				Gizmos.DrawWireSphere(Vector3.zero, circleCollider.radius);
		}
	}
}