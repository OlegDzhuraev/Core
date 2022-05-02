using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneOne.Core.UI
{
	/// <summary> Layered selectable is a simplier version of toggle group, which is not require extra mono component, using layer value for grouping. </summary>
	public sealed class LayeredSelectable : MonoBehaviour, IPointerClickHandler // todo support not only ui?..
	{
		static readonly Dictionary<byte, LayeredSelectable> layerSelectedObj = new(); // todo support destruction of selectables
		
		[SerializeField] byte layer;
		
		ISelectionReceiver[] receivers;
		
		void Awake() => receivers = GetComponents<ISelectionReceiver>();

		public void Select() => SetSelected(true);

		void SetSelected(bool isSelected)
		{
			if (isSelected)
			{
				if (layerSelectedObj.TryGetValue(layer, out var previous) && previous)
					previous.SetSelected(false);
				
				layerSelectedObj[layer] = this;
			}

			foreach (var receiver in receivers)
				receiver.SetState(isSelected);
		}
		
		public void OnPointerClick(PointerEventData eventData) => Select();
	}
}