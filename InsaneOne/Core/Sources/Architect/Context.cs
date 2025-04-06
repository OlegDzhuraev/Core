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
		static List<IContext<T>> receivers;

		static T context;

		/// <summary> Used to first initialize on loaded scene. Needed to be called once on level load (support multi-scenes, but should be called after all of them are loaded). </summary>
		public static void Initialize(T newContext, int capacity = 1024)
		{
			if (context != null)
				return;

			receivers = new List<IContext<T>>(capacity);
			context = newContext;

			for (var q = 0; q < SceneManager.sceneCount; q++)
			{
				var rootObjs = SceneManager.GetSceneAt(q).GetRootGameObjects();

				foreach (var root in rootObjs)
					CollectReceivers(root);
			}
		}

		/// <summary> Call on game stop </summary>
		public static void Dispose()
		{
			if (context == null)
				return;

			context = null;
			receivers.Clear();
		}

		/// <summary> Gets and initializes all context receivers from GameObject. </summary>
		static void CollectReceivers(GameObject go)
		{
			var newReceivers = go.GetComponentsInChildren<IContext<T>>(true);

			foreach (var newReceiver in newReceivers)
			{
				if (receivers.Contains(newReceiver)) // todo InsaneOne.Core: can consume a lot of resources when there a lot of objects. Optimize?
					continue;

				receivers.Add(newReceiver);
				newReceiver.ReloadContext(context);
			}
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

		/// <summary> Adds GameObject to context and applies context to all receivers on this GO. </summary>
		public static void Add(GameObject sceneObject) => CollectReceivers(sceneObject);
	}
}