using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary>Different frequently used extensions for engine to speed up development and reduce code length.</summary>
	public static class MiscExtensions 
	{
		/// <summary> Returns nearest to point object of type T from input list of objects. Can work incorrect in distances more than sqrt(float.MaxValue - 1).</summary>
		public static T GetNearestObjectOfType<T>(this Transform transform, List<T> objectsToSelectFrom) where T : Component
		{
			return GetNearestObjectOfType(transform.position, objectsToSelectFrom);
		}

		/// <summary> Returns nearest to point object of type T from input list of objects. Can work incorrect in distances more than sqrt(float.MaxValue - 1).</summary>
		public static T GetNearestObjectOfType<T>(this Vector3 position, List<T> objectsToSelectFrom) where T : Component
		{
			T nearestObject = null;
			var currentNearestSqrDistance = float.MaxValue - 1;

			for (int i = 0; i < objectsToSelectFrom.Count; i++)
			{
				var sqrDistance = (position - objectsToSelectFrom[i].transform.position).sqrMagnitude;

				if (sqrDistance < currentNearestSqrDistance)
				{
					nearestObject = objectsToSelectFrom[i];
					currentNearestSqrDistance = sqrDistance;
				}
			}

			return nearestObject;
		}

		public static T CheckComponent<T>(this GameObject go, bool isNeeded) where T : Component
		{
			var component = go.GetComponent<T>();
			
			if (isNeeded && !component)
				return go.gameObject.AddComponent<T>();
			
			if (isNeeded && component)
				return component;
			
			if (!isNeeded && component)
				GameObject.DestroyImmediate(component);

			return null;
		}
		
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			var component = go.GetComponent<T>();
			
			if (!component)
				return go.gameObject.AddComponent<T>();
			
			return component ? component : null;
		}
		
		public static T GetOrAddComponent<T>(this Behaviour behaviour) where T : Component
		{
			return behaviour.gameObject.GetOrAddComponent<T>();
		}
	}
}