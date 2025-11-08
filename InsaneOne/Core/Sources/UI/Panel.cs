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

namespace InsaneOne.Core.UI
{
	/// <summary>You can add this component to any window which should be modal with possibility to be shown and hidden.</summary>
	public sealed class Panel : Element<object>
	{
		[SerializeField] bool hideOnStart;
		
		public GameObject SelfObject => selfObject;
		public RectTransform RectTransform { get; private set; }
		public bool IsShown { get; private set; }

		bool wasShown;
		
		void Awake()
		{
			if (!selfObject)
				selfObject = gameObject;

			RectTransform = selfObject.GetComponent<RectTransform>();
		}

		void Start()
		{
			if (hideOnStart && !wasShown)
				Hide(true);
		}

		public override void Show()
		{
			if (IsShown)
				return;
			
			wasShown = true;
			IsShown = true;

			base.Show();
		}

		public void Hide(bool ignoreHiddenState = false)
		{
			if (!IsShown && !ignoreHiddenState)
				return;

			IsShown = false;
			base.Hide();
		}
	}
}