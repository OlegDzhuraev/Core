using UnityEngine;

namespace InsaneOne.Core.Architect
{
	public static class ContextSpawner
	{
		public static void Spawn<T1>(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)  where T1 : class
		{
			var go = Object.Instantiate(prefab, position, rotation, parent);

			Context<T1>.Add(go);
		}

		public static void Spawn<T1, T2>(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
			where T1 : class where T2 : class
		{
			var go = Object.Instantiate(prefab, position, rotation, parent);

			Context<T1>.Add(go);
			Context<T2>.Add(go);
		}

		public static void Spawn<T1, T2, T3>(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
			where T1 : class where T2 : class where T3 : class
		{
			var go = Object.Instantiate(prefab, position, rotation, parent);

			Context<T1>.Add(go);
			Context<T2>.Add(go);
			Context<T3>.Add(go);
		}

		public static void Spawn<T1, T2, T3, T4>(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
			where T1 : class where T2 : class where T3 : class where T4 : class
		{
			var go = Object.Instantiate(prefab, position, rotation, parent);

			Context<T1>.Add(go);
			Context<T2>.Add(go);
			Context<T3>.Add(go);
			Context<T4>.Add(go);
		}
	}
}