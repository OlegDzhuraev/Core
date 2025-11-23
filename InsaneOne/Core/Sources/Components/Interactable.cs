using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Components
{
	/// <summary> Represents some interactable object (button, for example). Sends WasInteracted event when was triggered by some interactor. </summary>
	[DisallowMultipleComponent]
	public class Interactable : MonoBehaviour
	{
		public event Action WasInteracted;

		readonly List<object> interactionBlockers = new ();

		public virtual bool TryInteract()
		{
			if (interactionBlockers.Count > 0)
				return false;

			WasInteracted?.Invoke();
			
			return true;
		}

		public void AddBlocker(object blocker)
		{
			if (!interactionBlockers.Contains(blocker))
				interactionBlockers.Add(blocker);
		}

		public void RemoveBlocker(object blocker) => interactionBlockers.Remove(blocker);

		public bool HasBlockers() => interactionBlockers.Count > 0;
	}
}