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

using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Scene view toolbar overlay hosting the Object Placer activation toggle, same as Unity's own Scene view overlays. </summary>
	[Overlay(typeof(SceneView), Id, "Object Placer", defaultDisplay: true)]
	public class ObjectPlacerToolOverlay : ToolbarOverlay
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay";

		ObjectPlacerToolOverlay() : base(ObjectPlacerActiveToggle.Id) { }
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ObjectPlacerActiveToggle : EditorToolbarToggle
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay.ActiveToggle";

		public ObjectPlacerActiveToggle()
		{
			text = "Object Placer";
			tooltip = "Toggles the Object Placer tool. While active, clicking a scene collider places the selected palette prefab.";

			SetValueWithoutNotify(ObjectPlacerToolState.IsActive);
			this.RegisterValueChangedCallback(OnToggleChanged);

			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
			RegisterCallback<DetachFromPanelEvent>(_ => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged);
		}

		void OnToggleChanged(ChangeEvent<bool> ev)
		{
			ObjectPlacerToolState.SetActive(ev.newValue);

			if (ev.newValue)
				ObjectPlacerWindow.Open();
		}
		void OnStateActiveChanged(bool value) => SetValueWithoutNotify(value);
	}
}
