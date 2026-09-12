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
	/// <summary> Scene view toolbar overlay hosting a shortcut to open the Arrange On Scene tool window, plus a
	/// dropdown picking which mode it opens in - same as Unity's own Scene view overlays, and the same shape as
	/// ObjectPlacerToolOverlay. </summary>
	[Overlay(typeof(SceneView), Id, "Arrange On Scene", defaultDisplay: true)]
	public class ArrangeOnSceneToolOverlay : ToolbarOverlay
	{
		public const string Id = "InsaneOne.LevelDesign.ArrangeOnSceneOverlay";

		ArrangeOnSceneToolOverlay() : base(ArrangeOnSceneOpenButton.Id, ArrangeOnSceneModeDropdown.Id) { }
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ArrangeOnSceneOpenButton : EditorToolbarButton
	{
		public const string Id = "InsaneOne.LevelDesign.ArrangeOnSceneOverlay.OpenButton";

		public ArrangeOnSceneOpenButton()
		{
			text = "Open";
			tooltip = "Opens the Arrange On Scene tool window, in the mode picked by the dropdown next to this button.";

			clicked += ArrangeOnSceneWindow.Init;
		}
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ArrangeOnSceneModeDropdown : VisualElement
	{
		public const string Id = "InsaneOne.LevelDesign.ArrangeOnSceneOverlay.ModeDropdown";

		readonly EnumField modeField;

		public ArrangeOnSceneModeDropdown()
		{
			modeField = new EnumField(ArrangeOnSceneToolState.Mode) { style = { minWidth = 90 } };
			modeField.tooltip = "Mode the Arrange On Scene window opens in (and switches to live, if it's already open).";
			Add(modeField);

			modeField.RegisterValueChangedCallback(OnModeChanged);

			RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
			RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
		}

		void OnModeChanged(ChangeEvent<Enum> ev) => ArrangeOnSceneToolState.SetMode((ArrangeOnSceneMode) ev.newValue);

		void OnAttachedToPanel(AttachToPanelEvent ev)
		{
			OnStateModeChanged(ArrangeOnSceneToolState.Mode);
			ArrangeOnSceneToolState.ModeChanged += OnStateModeChanged;
		}

		void OnDetachedFromPanel(DetachFromPanelEvent ev) => ArrangeOnSceneToolState.ModeChanged -= OnStateModeChanged;

		void OnStateModeChanged(ArrangeOnSceneMode mode) => modeField.SetValueWithoutNotify(mode);
	}
}
