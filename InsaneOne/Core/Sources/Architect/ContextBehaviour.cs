using UnityEngine;

namespace InsaneOne.Core.Architect
{
	/// <summary> Use this instead MonoBehaviour, if your component should receive some game context. It can use only one context per time.</summary>
	public abstract class ContextBehaviour<T> : MonoBehaviour
	{
		public T Context { get; private set; }

		public void ReloadContext(T newContext)
		{
			Context = newContext;
			OnContextReloaded();
		}
		
		protected virtual void OnContextReloaded() { }
	}
}