using UnityEngine;

namespace InsaneOne.Core
{
	///<summary> Draws wire-gizmo with selected color and form. Useful to show primary objects in editor when them isn't selected.</summary>
	public class GizmoHandle : MonoBehaviour
	{
		enum Form
		{
			Box,
			Sphere
		}

		enum DrawType
		{
			Wired,
			WiredAndFilled
		}
		
		[SerializeField] Color gizmoColor = Color.red;
		[SerializeField] Form form = Form.Box;
		[SerializeField] DrawType drawType = DrawType.Wired;
		
		void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = gizmoColor;

			if (form == Form.Sphere)
				DrawSphere(drawType);
			else
				DrawCube(drawType);
		}

		void DrawSphere(DrawType type)
		{
			if (type == DrawType.WiredAndFilled)
				Gizmos.DrawSphere(Vector3.zero, 0.5f);
			
			Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
		}

		void DrawCube(DrawType type)
		{	
			if (type == DrawType.WiredAndFilled)
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
			
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}