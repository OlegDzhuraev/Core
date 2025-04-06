using UnityEngine;

namespace InsaneOne.Core.Architect
{
	/// <summary> Use this instead MonoBehaviour, if your component should receive some game context. It can hold only one context type.</summary>
	public abstract class ContextBehaviour<T> : MonoBehaviour, IContext<T>
	{
		public T Context { get; set; }

		public void OnContextReloaded(T newContext)
		{
			Context = newContext;
		}
	}
}