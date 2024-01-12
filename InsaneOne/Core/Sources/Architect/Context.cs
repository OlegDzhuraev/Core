using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneOne.Core.Architect
{
	/// <summary> Use this class to provide some shared Context to any MonoBehaviours.
	/// <para>You need to Initialize this before all other components at scene load.</para>
	/// <para>Pass some Context data-class to the Initialize method, and it will be providen to
	/// all classes, using ContextBehaviour with the same context type.</para></summary>
	public static class Context<T> where T : class
	{
		static readonly List<ContextBehaviour<T>> receivers = new List<ContextBehaviour<T>>(1024);
		static T context;
		
		/// <summary> Used to first initialize on loaded scene. Needed to be called once on level load (support multiscenes, but should be called after all of them are loaded). </summary>
		public static void Initialize(T newContext)
		{
			receivers.Clear();
			context = newContext;

			for (var q = 0; q < SceneManager.sceneCount; q++)
			{
				var rootObjs = SceneManager.GetSceneAt(q).GetRootGameObjects();

				foreach (var root in rootObjs)
					CollectReceivers(root);
			}
		}

		/// <summary> Gets and initializes all context receivers from GameObject. </summary>
		static void CollectReceivers(GameObject go)
		{
			var newReceivers = go.GetComponentsInChildren<ContextBehaviour<T>>(true);
			receivers.AddRange(newReceivers);
					
			foreach (var receiver in newReceivers)
				receiver.ReloadContext(context);
		}
		
		/// <summary> Use when need to change context on the scene for actual context receivers. </summary>
		public static void Reset(T newContext)
		{
			context = newContext;

			for (var q = receivers.Count - 1; q >= 0; q--)
			{
				var receiver = receivers[q];
				
				if (receiver == null)
				{
					receivers.RemoveAt(q);
					continue;
				}

				receiver.ReloadContext(context);
			}
		}

		/// <summary> Use this method instead of Instantiate, it provides context to the spawned object. </summary>
		public static void Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
		{
			var go = GameObject.Instantiate(prefab, position, rotation, parent);
			CollectReceivers(go);
		} 
	}
}