/*
 * Copyright 2025 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary> Note that current version will work correctly only with mouse input. </summary>
	public class SelectableIcon : Element<object>, ISelectionReceiver, IPointerEnterHandler, IPointerExitHandler
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