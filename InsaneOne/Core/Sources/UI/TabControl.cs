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
			if (tabs.Count != buttons.Count)
				CoreUnityLogger.I.Log($"[{nameof(TabControl)}] tabs and buttons count mismatch on [{name}]!", LogLevel.Warning);

			for (var i = 0; i < buttons.Count; i++)
			{
				var cachedIndex = i;

				if (buttons[i])
					buttons[i].onClick.AddListener(() => ShowTab(cachedIndex));
			}

			for (var i = 1; i < tabs.Count; i++)
			{
				if (tabs[i])
					tabs[i].SetActive(false);
			}

			if (tabs.Count > 0 && tabs[0])
				tabs[0].SetActive(true); // make sure the first tab is actually visible - shownTab is 0 by default
		}

		public void ShowTab(int number)
		{
			if (number < 0 || number >= tabs.Count)
			{
				CoreUnityLogger.I.Log($"[{nameof(TabControl)}] Invalid tab index {number} on [{name}]!", LogLevel.Warning);
				return;
			}

			if (number == shownTab) // avoid the hide/show churn and spurious events when re-clicking the already shown tab
				return;

			if (tabs[shownTab])
				tabs[shownTab].SetActive(false);

			tabs[number].SetActive(true);

			shownTab = number;

			TabWasShown?.Invoke(tabs[shownTab]);
		}
	}
}