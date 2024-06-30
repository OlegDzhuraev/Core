using System;
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

		/// <summary> Casts a ray to mouse position from camera (for 3d only). </summary>
		public static bool CastCameraToMouseRay(int layerMask, out RaycastHit hit, int distance = 1000)
		{
			var ray = MainCamera.Cached.ScreenPointToRay(Input.mousePosition);
			return Physics.Raycast(ray.origin, ray.direction, out hit, distance, layerMask);
		}

		/// <summary>Gets components of type T from all physical objects in radius. Provide list to output results. Note that it will be overriden with new values.</summary>
		public static void GetObjectsOfTypeInSphere<T>(Vector3 position, float radius, List<T> output, int layerMask = Physics.AllLayers)
		{
			output.Clear();
			var size = Physics.OverlapSphereNonAlloc(position, radius, searchColliders, layerMask);

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders[i].GetComponent<T>();

				if (target != null)
					output.Add(target);
			}
		}
		
		/// <summary>Gets components of type T from all 2d physical objects in radius.  Provide list to output results. Note that it will be overriden with new values.</summary>
		public static void GetObjectsOfTypeIn2DCircle<T>(Vector2 position, float radius, List<T> output, int layerMask = Physics2D.AllLayers)
		{
			output.Clear();
			var size = Physics2D.OverlapCircleNonAlloc(position, radius, searchColliders2D, layerMask);

			for (int i = 0; i < size; i++)
			{
				var target = searchColliders2D[i].GetComponent<T>();

				if (target != null)
					output.Add(target);
			}
		}

		/// <summary>Gets components of type T from all 3d physical objects in radius. Applies input callback action for each found object.</summary>
		public static void ApplyToObjectsInSphere<T>(Vector3 position, float radius, Action<T> action, int layerMask = Physics.AllLayers)
		{
			var foundObjects = new List<T>();
			GetObjectsOfTypeInSphere(position, radius, foundObjects, layerMask);

			foreach (var foundObject in foundObjects)
				action.Invoke(foundObject);
		}

		/// <summary>Gets components of type T from all 2d physical objects in radius. Applies input callback action for each found object.</summary>
		public static void ApplyToObjectsIn2DCircle<T>(Vector2 position, float radius, Action<T> action, int layerMask = Physics2D.AllLayers)
		{
			var foundObjects = new List<T>();
			GetObjectsOfTypeIn2DCircle(position, radius, foundObjects, layerMask);

			foreach (var foundObject in foundObjects)
				action.Invoke(foundObject);
		}

		public static bool IsLayerInLayerMask(int layer, int layerMask) => layerMask == (layerMask | (1 << layer));
	}
}