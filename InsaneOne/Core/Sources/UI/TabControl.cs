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
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary>Tab control. Useful for windows like settings which requires several tabs.</summary>
	public sealed class TabControl : Element<object>
	{
		public event Action<GameObject> TabWasShown;
		
		[SerializeField] List<GameObject> tabs = new ();
		[SerializeField] List<Button> buttons = new ();

		int shownTab;

		void Start()
		{
			for (var i = 0; i < buttons.Count; i++)
			{
				var cachedIndex = i;
				buttons[i].onClick.AddListener(() => ShowTab(cachedIndex));
			}

			for (var i = 1; i < tabs.Count; i++)
				tabs[i].SetActive(false);
		}

		public void ShowTab(int number)
		{
			tabs[shownTab].SetActive(false);
			tabs[number].SetActive(true);

			shownTab = number;
			
			TabWasShown?.Invoke(tabs[shownTab]);
		}
	}
}