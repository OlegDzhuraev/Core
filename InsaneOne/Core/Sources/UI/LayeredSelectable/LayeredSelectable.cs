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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneOne.Core.UI
{
	/// <summary> Layered selectable is a simpler version of toggle group, which is not require extra mono component, using layer value for grouping. </summary>
	public sealed class LayeredSelectable : Element<object>, IPointerClickHandler // TODO InsaneOne: support not only ui?..
	{
		static readonly Dictionary<byte, LayeredSelectable> layerSelectedObj = new(); // TODO InsaneOne: support destruction of selectables
		
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