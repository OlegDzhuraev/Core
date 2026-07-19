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
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Placement settings block of the Object Placer tool: normal alignment and rotation/scale/position randomization.
	/// Values are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerPlacementSettingsSection : VisualElement
	{
		const string GroupStyleName = "group-box";

		const string MaxSlopeAngleKey = "InsaneOne.ObjectPlacer.MaxSlopeAngle";
		const string AlignToNormalKey = "InsaneOne.ObjectPlacer.AlignToNormal";
		const string RandomizeRotationKey = "InsaneOne.ObjectPlacer.RandomizeRotation";
		const string MaxRotationAngleKey = "InsaneOne.ObjectPlacer.MaxRotationAngle";
		const string RandomizeScaleKey = "InsaneOne.ObjectPlacer.RandomizeScale";
		const string ScaleRangeMinKey = "InsaneOne.ObjectPlacer.ScaleRangeMin";
		const string ScaleRangeMaxKey = "InsaneOne.ObjectPlacer.ScaleRangeMax";
		const string RandomizePositionKey = "InsaneOne.ObjectPlacer.RandomizePosition";
		const string MaxPositionOffsetKey = "InsaneOne.ObjectPlacer.MaxPositionOffset";
		const string SnapToGridKey = "InsaneOne.ObjectPlacer.SnapToGrid";
		const string GridSizeKey = "InsaneOne.ObjectPlacer.GridSize";

		public float MaxSlopeAngle => maxSlopeAngleField.value;

		public bool AlignToNormal => alignToNormalToggle.value;

		public bool RandomizeRotation => randomizeRotationToggle.value;
		public float MaxRotationAngle => maxRotationAngleField.value;

		public bool RandomizeScale => randomizeScaleToggle.value;
		public Vector2 ScaleRange => scaleRangeSlider.value;

		public bool RandomizePosition => randomizePositionToggle.value;
		public float MaxPositionOffset => maxPositionOffsetField.value;

		public bool SnapToGrid => snapToGridToggle.value;
		public float GridSize => gridSizeField.value;

		readonly Slider maxSlopeAngleField;

		readonly Toggle alignToNormalToggle;

		readonly Toggle randomizeRotationToggle;
		readonly Slider maxRotationAngleField;

		readonly Toggle randomizeScaleToggle;
		readonly Label scaleRangeLabel;
		readonly MinMaxSlider scaleRangeSlider;

		readonly Toggle randomizePositionToggle;
		readonly Slider maxPositionOffsetField;

		readonly Toggle snapToGridToggle;
		readonly Slider gridSizeField;

		public ObjectPlacerPlacementSettingsSection()
		{
			AddToClassList(GroupStyleName);

			Add(new Label("Placement Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			maxSlopeAngleField = new Slider("Max Surface Slope", 0f, 90f) { value = SessionState.GetFloat(MaxSlopeAngleKey, 90f), showInputField = true };
			maxSlopeAngleField.RegisterValueChangedCallback(ev => SessionState.SetFloat(MaxSlopeAngleKey, ev.newValue));
			Add(maxSlopeAngleField);

			alignToNormalToggle = new Toggle("Align Up To Surface Normal") { value = SessionState.GetBool(AlignToNormalKey, false) };
			alignToNormalToggle.RegisterValueChangedCallback(ev => SessionState.SetBool(AlignToNormalKey, ev.newValue));
			Add(alignToNormalToggle);

			randomizeRotationToggle = new Toggle("Randomize Rotation") { value = SessionState.GetBool(RandomizeRotationKey, false) };
			maxRotationAngleField = new Slider("Max Angle", 0f, 180f) { value = SessionState.GetFloat(MaxRotationAngleKey, 45f), showInputField = true };
			randomizeRotationToggle.RegisterValueChangedCallback(ev =>
			{
				SessionState.SetBool(RandomizeRotationKey, ev.newValue);
				maxRotationAngleField.style.display = GetDisplay(ev.newValue);
			});
			maxRotationAngleField.RegisterValueChangedCallback(ev => SessionState.SetFloat(MaxRotationAngleKey, ev.newValue));
			maxRotationAngleField.style.display = GetDisplay(randomizeRotationToggle.value);

			randomizeScaleToggle = new Toggle("Randomize Scale") { value = SessionState.GetBool(RandomizeScaleKey, false) };
			scaleRangeLabel = new Label { style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) } };
			scaleRangeSlider = new MinMaxSlider("Scale Range", SessionState.GetFloat(ScaleRangeMinKey, 0.8f), SessionState.GetFloat(ScaleRangeMaxKey, 1.2f), 0.1f, 5f);
			UpdateScaleRangeLabel(scaleRangeSlider.value);
			scaleRangeSlider.RegisterValueChangedCallback(OnScaleRangeChanged);
			randomizeScaleToggle.RegisterValueChangedCallback(OnRandomizeScaleChanged);
			scaleRangeLabel.style.display = GetDisplay(randomizeScaleToggle.value);
			scaleRangeSlider.style.display = GetDisplay(randomizeScaleToggle.value);

			randomizePositionToggle = new Toggle("Randomize Position") { value = SessionState.GetBool(RandomizePositionKey, false) };
			maxPositionOffsetField = new Slider("Max Offset", 0f, 5f) { value = SessionState.GetFloat(MaxPositionOffsetKey, 0.5f), showInputField = true };
			randomizePositionToggle.RegisterValueChangedCallback(ev =>
			{
				SessionState.SetBool(RandomizePositionKey, ev.newValue);
				maxPositionOffsetField.style.display = GetDisplay(ev.newValue);
			});
			maxPositionOffsetField.RegisterValueChangedCallback(ev => SessionState.SetFloat(MaxPositionOffsetKey, ev.newValue));
			maxPositionOffsetField.style.display = GetDisplay(randomizePositionToggle.value);

			snapToGridToggle = new Toggle("Snap To Grid") { value = SessionState.GetBool(SnapToGridKey, false) };
			gridSizeField = new Slider("Grid Size", 0.01f, 10f) { value = SessionState.GetFloat(GridSizeKey, 1f), showInputField = true };
			snapToGridToggle.RegisterValueChangedCallback(ev =>
			{
				SessionState.SetBool(SnapToGridKey, ev.newValue);
				gridSizeField.style.display = GetDisplay(ev.newValue);
			});
			gridSizeField.RegisterValueChangedCallback(ev => SessionState.SetFloat(GridSizeKey, ev.newValue));
			gridSizeField.style.display = GetDisplay(snapToGridToggle.value);

			Add(randomizeRotationToggle);
			Add(maxRotationAngleField);
			Add(randomizeScaleToggle);
			Add(scaleRangeLabel);
			Add(scaleRangeSlider);
			Add(randomizePositionToggle);
			Add(maxPositionOffsetField);
			Add(snapToGridToggle);
			Add(gridSizeField);
		}

		void OnScaleRangeChanged(ChangeEvent<Vector2> ev)
		{
			SessionState.SetFloat(ScaleRangeMinKey, ev.newValue.x);
			SessionState.SetFloat(ScaleRangeMaxKey, ev.newValue.y);
			UpdateScaleRangeLabel(ev.newValue);
		}

		void UpdateScaleRangeLabel(Vector2 range)
		{
			var x = Math.Round(range.x, 2);
			var y = Math.Round(range.y, 2);
			scaleRangeLabel.text = $"Min: {x} | Max: {y}";
		}

		void OnRandomizeScaleChanged(ChangeEvent<bool> ev)
		{
			SessionState.SetBool(RandomizeScaleKey, ev.newValue);
			var display = GetDisplay(ev.newValue);
			scaleRangeLabel.style.display = display;
			scaleRangeSlider.style.display = display;
		}

		static StyleEnum<DisplayStyle> GetDisplay(bool isEnabled) => new (isEnabled ? StyleKeyword.Auto : StyleKeyword.None);
	}
}
