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
	// todo InsaneOne: require/auto-add some layout and content size fitter on "content" object?
	[DisallowMultipleComponent]
	public sealed class Foldout : Element<object>
	{
		const float OpenedRotationZ = 180;
		const float ClosedRotationZ = 0;

		[SerializeField] Toggle toggle;
		[Tooltip("This content will be shown or hidden on foldout state change")]
		[SerializeField] GameObject content;
		[SerializeField] RectTransform openIconTransform;

		void Awake()
		{
			toggle.ViewModelChanged += OnToggleViewModelChanged;
		}

		void OnToggleViewModelChanged(ToggleViewModel toggleVm)
		{
			toggleVm.WasChanged += OnToggleChanged;
		}

		void OnToggleChanged(bool isOn) => SetState(isOn);

		void SetState(bool isOpened)
		{
			var angles = openIconTransform.localEulerAngles;
			angles.z = isOpened ? OpenedRotationZ : ClosedRotationZ;
			openIconTransform.localEulerAngles = angles;
		}
	}
}