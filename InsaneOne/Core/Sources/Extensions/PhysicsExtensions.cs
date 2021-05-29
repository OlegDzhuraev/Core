using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
	public static class PhysicsExtensions
	{
		/// <summary>Gets components of type T from all physical objects in radius.</summary>
		public static List<T> GetObjectsOfTypeInSphere<T>(Vector3 position, float radius)
		{
			var colliders = Physics.OverlapSphere(position, radius); // todo make it non-alloc
			var foundObjectsOfType = new List<T>();

			for (int i = 0; i < colliders.Length; i++)
			{
				var foundComponent = colliders[i].GetComponent<T>();

				if (foundComponent != null)
					foundObjectsOfType.Add(foundComponent);
			}

			return foundObjectsOfType;
		}
		
		/// <summary>Gets components of type T from all 2d physical objects in radius.</summary>
		public static List<T> GetObjectsOfTypeIn2DCircle<T>(Vector3 position, float radius)
		{
			var colliders = Physics2D.OverlapCircleAll(position, radius); // todo make it non-alloc
			var foundObjectsOfType = new List<T>();

			for (int i = 0; i < colliders.Length; i++)
			{
				var foundComponent = colliders[i].GetComponent<T>();

				if (foundComponent != null)
					foundObjectsOfType.Add(foundComponent);
			}

			return foundObjectsOfType;
		}

		public static bool IsLayerInLayerMask(int layer, int layerMask) => layerMask == (layerMask | (1 << layer));
	}
}