using UnityEngine;

namespace InsaneOne.Core.UI
{
	public class SelectableActivation : MonoBehaviour, ISelectionReceiver
	{
		[SerializeField] GameObject objToActivate;
		
		void ISelectionReceiver.SetState(bool isSelected) => objToActivate.SetActive(isSelected);
	}
}