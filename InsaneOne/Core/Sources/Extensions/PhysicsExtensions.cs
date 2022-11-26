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
		
		/// <summary>Gets components of type T from all physical objects in radius.</summary>
		public static List<T> GetObjectsOfTypeInSphere<T>(Vector3 position, float radius)
		{
			var size = Physics.OverlapSphereNonAlloc(position, radius, searchColliders);
			var result = new List<T>();

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders[i].GetComponent<T>();

				if (target != null)
					result.Add(target);
			}

			return result;
		}
		
		/// <summary>Gets components of type T from all 2d physical objects in radius.</summary>
		public static List<T> GetObjectsOfTypeIn2DCircle<T>(Vector3 position, float radius)
		{
			var size = Physics2D.OverlapCircleNonAlloc(position, radius, searchColliders2D);
			var result = new List<T>();

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders2D[i].GetComponent<T>();

				if (target != null)
					result.Add(target);
			}

			return result;
		}

		public static bool IsLayerInLayerMask(int layer, int layerMask) => layerMask == (layerMask | (1 << layer));
	}
}