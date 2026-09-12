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
	/// <summary> Shared Mode state for the Arrange On Scene tool, kept in sync between the tool window's own Mode
	/// dropdown and the Scene view toolbar's shortcut dropdown. Backed by SessionState, so it survives domain
	/// reloads within the same Editor session. </summary>
	public static class ArrangeOnSceneToolState
	{
		const string ModeKey = "InsaneOne.ArrangeOnScene.Mode";

		public static event Action<ArrangeOnSceneMode> ModeChanged;

		public static ArrangeOnSceneMode Mode
		{
			get => (ArrangeOnSceneMode) SessionState.GetInt(ModeKey, (int) ArrangeOnSceneMode.Align);
			private set => SessionState.SetInt(ModeKey, (int) value);
		}

		public static void SetMode(ArrangeOnSceneMode value)
		{
			if (Mode == value)
				return;

			Mode = value;
			ModeChanged?.Invoke(value);
		}
	}
}
