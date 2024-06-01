using InsaneOne.Core.Components;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> 2D Trigger for interactable component, allows to interact on enter or key press when staying in trigger. </summary>
	public sealed class TagTriggerInteractable2D : BaseTriggerInteractable2D
	{
		[SerializeField] bool interactOnEnter;
		[SerializeField] KeyCode defaultActionKey = KeyCode.E;
		[SerializeField] string tagForCheck = "Player";

		void Update()
		{
			if (interactOnEnter || !IsTargetNear)
				return;

			if (Input.GetKeyDown(defaultActionKey))
				Interact();
		}

		protected override void OnTriggerEnter2D(Collider2D other)
		{
			if (!other.gameObject.CompareTag(tagForCheck))
				return;

			IsTargetNear = true;

			if (interactOnEnter)
				Interact();
		}

		protected override void OnTriggerExit2D(Collider2D other)
		{
			if (other.gameObject.CompareTag(tagForCheck))
				IsTargetNear = false;
		}
	}
}