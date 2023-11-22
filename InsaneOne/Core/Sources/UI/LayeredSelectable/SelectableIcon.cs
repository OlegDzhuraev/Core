using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary> Note that current version will work correctly only with mouse input. </summary>
	public class SelectableIcon : MonoBehaviour, ISelectionReceiver, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] Image iconImage;
		[SerializeField] Sprite icon, hoveredIcon, selectedIcon;
	
		Sprite hoverPreviousIcon;
		
		void Awake() => iconImage.sprite = icon;

		void ISelectionReceiver.SetState(bool isSelected)
		{
			if (isSelected)
				hoverPreviousIcon = iconImage.sprite = selectedIcon;
			else 
				iconImage.sprite = icon;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hoverPreviousIcon = iconImage.sprite;
			iconImage.sprite = hoveredIcon;
		}

		public void OnPointerExit(PointerEventData eventData) => iconImage.sprite = hoverPreviousIcon;
	}
}