using UnityEngine;

namespace InsaneOne.Core.Components
{
	/// <summary> 2D Trigger for interactable component. This is base abstract class to make your own ones from. </summary>
	public abstract class BaseTriggerInteractable2D : MonoBehaviour
	{
		/// <summary> Is trigger-affector target in trigger. Set this in OnTriggerEnter/Exit. </summary>
		public bool IsTargetNear { get; protected set; }

		protected Interactable interactable;

		protected virtual void Awake()
		{
			interactable = GetComponent<Interactable>();
		}

		protected virtual void Interact() => interactable.TryInteract();

		protected abstract void OnTriggerEnter2D(Collider2D other);
		protected abstract void OnTriggerExit2D(Collider2D other);
	}
}