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

using System;
using System.Collections.Generic;
using InsaneOne.Core.Utility;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> Use it for some icons lists, where some data should be passed to some icon.
	/// It can be inventory items, rts units buy icons, skills icons, etc. </summary>
	public abstract class IconsPanel<TIcon, TData> : MonoBehaviour where TIcon : MonoBehaviour where TData : class
	{
		public event Action WasRedrawn;
		
		/// <summary> Use to override default icon template, which can be setup from the inspector. </summary>
		public GameObject IconTemplate
		{
			get => iconTemplate;
			set => iconTemplate = value;
		}
		
		[Tooltip("Icons will be placed onto this panel. Recommended to use some Layout Group.")]
		[SerializeField] protected Transform panel;
		[Tooltip("Default icon template to spawn. Can be overriden via code by using public property.")]
		[SerializeField] protected GameObject iconTemplate;

		protected readonly List<TIcon> drawnIcons = new ();
		
		/// <summary> Use to redraw icons with a new datas. </summary>
		public void Redraw(List<TData> datas)
		{
			ClearDrawn();

			foreach (var data in datas)
			{
				if (data == null)
				{
					CoreUnityLogger.I.Log($"Some data in input list is NULL on the [{gameObject.name}] IconsPanel! Skipped.", LogLevel.Warning);
					continue;
				}
				
				var iconGo = Instantiate(iconTemplate, panel);
				var icon = iconGo.GetComponent<TIcon>();
				
				drawnIcons.Add(icon);
				InitIcon(icon, data);
			}
			
			OnRedraw();
			WasRedrawn?.Invoke();
		}

		protected virtual void OnRedraw() { }

		protected virtual void ClearDrawn()
		{
			foreach (var icon in drawnIcons)
				Destroy(icon.gameObject);

			drawnIcons.Clear();
		}

		/// <summary> Apply all icon initialization here. Load data into icon, etc. </summary>
		protected abstract void InitIcon(TIcon icon, TData data);
	}
}