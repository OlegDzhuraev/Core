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
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Scene view toolbar overlay hosting the Object Placer activation toggle and mode dropdown, same as Unity's own Scene view overlays. </summary>
	[Overlay(typeof(SceneView), Id, "Object Placer", defaultDisplay: true)]
	public class ObjectPlacerToolOverlay : ToolbarOverlay
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay";

		ObjectPlacerToolOverlay() : base(ObjectPlacerActiveToggle.Id, ObjectPlacerModeDropdown.Id) { }
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ObjectPlacerActiveToggle : EditorToolbarToggle
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay.ActiveToggle";

		public ObjectPlacerActiveToggle()
		{
			text = "Enable";
			tooltip = "Toggles the Object Placer tool. While active, clicking a scene collider places the selected palette prefab.";

			SetValueWithoutNotify(ObjectPlacerToolState.IsActive);
			this.RegisterValueChangedCallback(OnToggleChanged);

			RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
			RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
		}

		void OnToggleChanged(ChangeEvent<bool> ev)
		{
			ObjectPlacerToolState.SetActive(ev.newValue);

			if (ev.newValue)
				ObjectPlacerWindow.Open();
		}

		void OnAttachedToPanel(AttachToPanelEvent ev)
		{
			OnStateActiveChanged(ObjectPlacerToolState.IsActive);
			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
		}

		void OnDetachedFromPanel(DetachFromPanelEvent ev) => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged;

		void OnStateActiveChanged(bool value) => SetValueWithoutNotify(value);
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ObjectPlacerModeDropdown : VisualElement
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay.ModeDropdown";

		readonly EnumField modeField;

		public ObjectPlacerModeDropdown()
		{
			modeField = new EnumField(ObjectPlacerToolState.Mode) { style = { minWidth = 110 } };
			modeField.tooltip = "Object Placer interaction mode.";
			Add(modeField);

			SetEnabled(ObjectPlacerToolState.IsActive);

			modeField.RegisterValueChangedCallback(OnModeChanged);

			RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
			RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
		}

		void OnModeChanged(ChangeEvent<Enum> ev) => ObjectPlacerToolState.SetMode((ObjectPlacerMode)ev.newValue);

		void OnAttachedToPanel(AttachToPanelEvent ev)
		{
			OnStateModeChanged(ObjectPlacerToolState.Mode);
			OnStateActiveChanged(ObjectPlacerToolState.IsActive);

			ObjectPlacerToolState.ModeChanged += OnStateModeChanged;
			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
		}

		void OnDetachedFromPanel(DetachFromPanelEvent ev)
		{
			ObjectPlacerToolState.ModeChanged -= OnStateModeChanged;
			ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged;
		}

		void OnStateModeChanged(ObjectPlacerMode mode) => modeField.SetValueWithoutNotify(mode);
		void OnStateActiveChanged(bool value) => SetEnabled(value);
	}
}
