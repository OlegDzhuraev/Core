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
		static HashSet<IContext<T>> receiverSet;

		static T context;

		/// <summary> Returns the currently active context value, or null if Context&lt;T&gt; wasn't initialized yet. </summary>
		public static T Get() => context;

		/// <summary> Used to first initialize on loaded scene. Needed to be called once on level load (support multi-scenes, but should be called after all of them are loaded). </summary>
		public static void Initialize(T newContext, int capacity = 1024)
		{
			if (context != null)
			{
				Debug.LogWarning($"[Context<{typeof(T).Name}>] Already initialized, ignoring repeated Initialize call - call Dispose() first if you need to reinitialize with a new context.");
				return;
			}

			receivers = new List<IContext<T>>(capacity);
			receiverSet = new HashSet<IContext<T>>(capacity);
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
			receiverSet.Clear();
		}

		/// <summary> Gets and initializes all context receivers from GameObject. </summary>
		static void CollectReceivers(GameObject go)
		{
			var newReceivers = go.GetComponentsInChildren<IContext<T>>(true);

			foreach (var newReceiver in newReceivers)
			{
				if (!receiverSet.Add(newReceiver)) // Add returns false if it was already present - O(1) instead of List.Contains' O(n)
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
					receiverSet.Remove(receiver);
					continue;
				}

				receiver.ReloadContext(context);
			}
		}

		/// <summary> Adds GameObject to context and applies context to all receivers on this GO. </summary>
		public static void Add(GameObject sceneObject) => CollectReceivers(sceneObject);
	}
}