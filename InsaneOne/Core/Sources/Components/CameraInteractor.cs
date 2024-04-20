using System;
using UnityEngine;

namespace InsaneOne.Core.Components
{
	/// <summary> Default interaction class, some methods can be overriden with custom logics. </summary>
	[DisallowMultipleComponent]
	public class CameraInteractor : MonoBehaviour
	{
		public event Action<Interactable> Focused, Unfocused;

		public LayerMask LayerMask
		{
			get => layerMask;
			set => layerMask = value;
		}

		public float InteractDistance
		{
			get => interactDistance;
			set => interactDistance = value;
		}

		[SerializeField] float interactDistance = 0.5f;
		[SerializeField] LayerMask layerMask;
		[SerializeField] KeyCode defaultActionKey = KeyCode.E;
		
		Interactable focusedInteractable;

		void Update()
		{
			if (!PhysicsExtensions.CastRayFromCamera(layerMask, out var hit))
				return;

			if (hit.distance > interactDistance)
			{
				RemoveFocus();
				return;
			}

			if (hit.collider && hit.collider.TryGetComponent<Interactable>(out var interactable))
			{
				if (focusedInteractable != interactable)
					Focused?.Invoke(focusedInteractable);

				focusedInteractable = interactable;
			}
			else
			{
				RemoveFocus();
			}

			if (focusedInteractable && IsActionInputted())
				focusedInteractable.TryInteract();
		}

		void RemoveFocus()
		{
			if (!focusedInteractable)
				return;
			
			Unfocused?.Invoke(focusedInteractable);
			focusedInteractable = null;
		}
		
		protected virtual bool IsActionInputted() => Input.GetKeyDown(defaultActionKey);
	}
}