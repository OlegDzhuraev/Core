using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
	public static class PhysicsExtensions
	{
		static Collider[] searchColliders = new Collider[64];
		static Collider2D[] searchColliders2D = new Collider2D[64];

		public static void SetMaxSearchColliders(uint value)
		{
			searchColliders = new Collider[value];
			searchColliders2D = new Collider2D[value];
		}

		/// <summary> Casts a ray to mouse position from camera. </summary>
		public static bool CastRayFromCamera(int layerMask, out RaycastHit hit, int distance = 1000)
		{
			var ray = MainCamera.Cached.ScreenPointToRay(Input.mousePosition);
			return Physics.Raycast(ray.origin, ray.direction, out hit, distance, layerMask);
		}

		/// <summary>Gets components of type T from all physical objects in radius. Provide list to output results. Note that it will be overriden with new values.</summary>
		public static void GetObjectsOfTypeInSphere<T>(Vector3 position, float radius, List<T> output)
		{
			output.Clear();
			var size = Physics.OverlapSphereNonAlloc(position, radius, searchColliders);

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders[i].GetComponent<T>();

				if (target != null)
					output.Add(target);
			}
		}
		
		/// <summary>Gets components of type T from all 2d physical objects in radius.  Provide list to output results. Note that it will be overriden with new values.</summary>
		public static List<T> GetObjectsOfTypeIn2DCircle<T>(Vector3 position, float radius, List<T> output)
		{
			output.Clear();
			var size = Physics2D.OverlapCircleNonAlloc(position, radius, searchColliders2D);

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders2D[i].GetComponent<T>();

				if (target != null)
					output.Add(target);
			}

			return output;
		}

		public static bool IsLayerInLayerMask(int layer, int layerMask) => layerMask == (layerMask | (1 << layer));
	}
}