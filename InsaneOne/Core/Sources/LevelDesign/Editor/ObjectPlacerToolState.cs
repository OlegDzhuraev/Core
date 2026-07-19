/*
 * Copyright 2026 Oleg Dzhuraev <godlikeaurora@gmail.com>
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
using UnityEditor;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Shared activation state for the Object Placer tool, kept in sync between the tool window and the Scene view toolbar toggle.
	/// Backed by SessionState, so it survives domain reloads within the same Editor session. </summary>
	public static class ObjectPlacerToolState
	{
		const string ActiveKey = "InsaneOne.ObjectPlacer.Active";

		public static event Action<bool> ActiveChanged;

		public static bool IsActive
		{
			get => SessionState.GetBool(ActiveKey, false);
			private set => SessionState.SetBool(ActiveKey, value);
		}

		public static void SetActive(bool value)
		{
			if (IsActive == value)
				return;

			IsActive = value;
			ActiveChanged?.Invoke(value);
		}
	}
}
