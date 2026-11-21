using InsaneOne.Core.Components;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> 2D Trigger for interactable component, allows to interact on enter or key press when staying in trigger. </summary>
	[DisallowMultipleComponent]
	public sealed class TagTriggerInteractable2D : BaseTriggerInteractable2D
	{
		[SerializeField] bool interactOnEnter;
		[SerializeField] KeyCode defaultActionKey = KeyCode.E;
		[SerializeField] string tagForCheck = "Player";

		int targetsInsideCount;

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

			targetsInsideCount++;

			IsTargetNear = true;

			if (interactOnEnter)
				Interact();
		}

		protected override void OnTriggerExit2D(Collider2D other)
		{
			if (!other.gameObject.CompareTag(tagForCheck))
				return;

			targetsInsideCount = Mathf.Max(0, targetsInsideCount - 1);
			IsTargetNear = targetsInsideCount > 0; // one collider exit doesn't mean the target left - it may have several colliders inside
		}
	}
}