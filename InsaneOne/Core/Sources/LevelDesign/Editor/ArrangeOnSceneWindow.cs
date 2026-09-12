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
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Aligns, evenly distributes, arranges in a circle or stacks with fixed spacing the positions of the selected scene GameObjects relative to each other, similar to the align tools found in 3D editors.
	/// The actual position/rotation math lives in ArrangeOnSceneMath - this class is only the EditorWindow/UI Toolkit/Scene-view wiring around it. </summary>
	public class ArrangeOnSceneWindow : EditorWindow
	{
		const string ToggleButtonRowClass = "toggle-button-row";
		const string ToggleButtonClass = "toggle-button";
		const string ToggleButtonFirstClass = "toggle-button-first";
		const string ToggleButtonLastClass = "toggle-button-last";
		const string ToggleButtonActiveClass = "toggle-button-active";
		const string ApplyButtonClass = "arrange-apply-button";
		const string AlignUndoName = "Arrange On Scene - Align";
		const string DistributeUndoName = "Arrange On Scene - Distribute";
		const string CircleUndoName = "Arrange On Scene - Circle";
		const string StackUndoName = "Arrange On Scene - Stack";
		const string RaycastUndoName = "Arrange On Scene - Raycast To Ground";

		// Session-persisted (survives domain reload, not Editor restart) - see the level-design-editor-tools skill.
		// Mode itself is persisted by ArrangeOnSceneToolState instead (under the same key), since it's also driven
		// by the Scene view overlay dropdown and needs to stay in sync between both.
		const string AlignAxesKey = "InsaneOne.ArrangeOnScene.AlignAxes";
		const string AlignAnchorKey = "InsaneOne.ArrangeOnScene.AlignAnchor";
		const string AlignTargetGlobalIdKey = "InsaneOne.ArrangeOnScene.AlignTargetGlobalId";
		const string DistributeAxesKey = "InsaneOne.ArrangeOnScene.DistributeAxes";
		const string CircleRadiusModeKey = "InsaneOne.ArrangeOnScene.CircleRadiusMode";
		const string CircleRadiusKey = "InsaneOne.ArrangeOnScene.CircleRadius";
		const string CircleCenterModeKey = "InsaneOne.ArrangeOnScene.CircleCenterMode";
		const string CircleCenterTargetGlobalIdKey = "InsaneOne.ArrangeOnScene.CircleCenterTargetGlobalId";
		const string CircleAngleOffsetKey = "InsaneOne.ArrangeOnScene.CircleAngleOffset";
		const string CircleRotationModeKey = "InsaneOne.ArrangeOnScene.CircleRotationMode";
		const string StackAxesKey = "InsaneOne.ArrangeOnScene.StackAxes";
		const string StackSpacingKey = "InsaneOne.ArrangeOnScene.StackSpacing";
		const string RaycastDirectionKey = "InsaneOne.ArrangeOnScene.RaycastDirection";
		const string RaycastDirectionTargetGlobalIdKey = "InsaneOne.ArrangeOnScene.RaycastDirectionTargetGlobalId";
		const string RaycastAlignWithNormalKey = "InsaneOne.ArrangeOnScene.RaycastAlignWithNormal";
		const string RaycastLayerMaskKey = "InsaneOne.ArrangeOnScene.RaycastLayerMask";

		const int MinObjectsToArrange = 2;
		const float DefaultCircleRadius = 3f;
		const float MinCircleRadius = 0.01f;
		const float DefaultCircleAngleOffset = 0f;
		const float MaxCircleAngleOffset = 359f;
		const float DefaultStackSpacing = 2f;
		const float RaycastMaxDistance = 1000f;

		ArrangeOnSceneMode mode;
		ArrangeOnSceneMath.Axes alignAxes;
		ArrangeOnSceneMath.AlignAnchor alignAnchor;
		Transform alignTarget;
		ArrangeOnSceneMath.Axes distributeAxes;
		ArrangeOnSceneMath.CircleRadiusMode circleRadiusMode;
		float circleRadius;
		ArrangeOnSceneMath.CircleCenterMode circleCenterMode;
		Transform circleCenterTarget;
		float circleAngleOffset;
		ArrangeOnSceneMath.CircleRotationMode circleRotationMode;
		ArrangeOnSceneMath.Axes stackAxes;
		float stackSpacing;
		ArrangeOnSceneMath.RaycastDirection raycastDirection;
		Transform raycastDirectionTarget;
		bool raycastAlignWithNormal;
		LayerMask raycastLayerMask;

		HelpBox infoBox;
		VisualElement alignBox, distributeBox, circleBox, stackBox, raycastBox;
		DropdownField modeField;
		Button applyBtn;

		[MenuItem("Tools/InsaneOne/Level Design/Arrange On Scene...")]
		internal static void Init()
		{
			var window = (ArrangeOnSceneWindow)GetWindow(typeof(ArrangeOnSceneWindow), false, "Arrange On Scene", true);
			window.Show();
		}

		void OnEnable()
		{
			SceneView.duringSceneGui += OnSceneGUI;
			Selection.selectionChanged += UpdateInfoBoxVisibility;
			ArrangeOnSceneToolState.ModeChanged += OnToolStateModeChanged;
		}

		void OnDisable()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
			Selection.selectionChanged -= UpdateInfoBoxVisibility;
			ArrangeOnSceneToolState.ModeChanged -= OnToolStateModeChanged;
		}

		// Keeps this window's Mode dropdown and previewed mode in sync with the Scene view overlay's own Mode
		// dropdown (ArrangeOnSceneModeDropdown), whichever of the two the user actually changes.
		void OnToolStateModeChanged(ArrangeOnSceneMode value)
		{
			mode = value;
			modeField?.SetValueWithoutNotify(modeField.choices[(int) value]);
			ApplyModeVisibility();
			SceneView.RepaintAll();
		}

		// Selection.selectionChanged doesn't reach here until CreateGUI has run at least once.
		void UpdateInfoBoxVisibility()
		{
			if (infoBox == null)
				return;

			infoBox.style.display = GetDisplay(Selection.transforms.Length < MinObjectsToArrange);
		}

		void CreateGUI()
		{
			var style = Resources.Load(LevelDesignToolStyles.StylesPath) as StyleSheet;
			var root = rootVisualElement;
			root.styleSheets.Add(style);
			root.AddToClassList("custom-tool-root");

			infoBox = new HelpBox("Select 2 or more objects first.", HelpBoxMessageType.Warning);
			UpdateInfoBoxVisibility();

			alignAxes = (ArrangeOnSceneMath.Axes) SessionState.GetInt(AlignAxesKey, (int) ArrangeOnSceneMath.Axes.None);
			alignAnchor = (ArrangeOnSceneMath.AlignAnchor) SessionState.GetInt(AlignAnchorKey, (int) ArrangeOnSceneMath.AlignAnchor.Center);

			alignBox = new VisualElement();
			alignBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			alignBox.Add(CreateSectionTitle("Align"));
			alignBox.Add(CreateDescription("Aligns the selection on the chosen axes to the min, center, max or a chosen target object."));

			alignBox.Add(CreateLabeledRow("Align Axes", BuildToggleButtonRow(
				AxesOptions,
				flag => (alignAxes & flag) != 0,
				flag =>
				{
					alignAxes ^= flag;
					SessionState.SetInt(AlignAxesKey, (int) alignAxes);
					SceneView.RepaintAll();
				})));

			var alignTargetField = new ObjectField("Target") { objectType = typeof(Transform), allowSceneObjects = true };
			alignTargetField.style.display = GetDisplay(alignAnchor == ArrangeOnSceneMath.AlignAnchor.Target);
			alignTargetField.RegisterValueChangedCallback(ev =>
			{
				alignTarget = ev.newValue as Transform;
				SaveTransform(AlignTargetGlobalIdKey, alignTarget);
				SceneView.RepaintAll();
			});
			LoadPersistedTransform(AlignTargetGlobalIdKey, alignTargetField, t => alignTarget = t);

			alignBox.Add(CreateLabeledRow("Align To", BuildToggleButtonRow(
				new (ArrangeOnSceneMath.AlignAnchor value, string label)[]
				{
					(ArrangeOnSceneMath.AlignAnchor.Min, "Min"), (ArrangeOnSceneMath.AlignAnchor.Center, "Center"),
					(ArrangeOnSceneMath.AlignAnchor.Max, "Max"), (ArrangeOnSceneMath.AlignAnchor.Target, "Target"),
				},
				value => alignAnchor == value,
				value =>
				{
					alignAnchor = value;
					SessionState.SetInt(AlignAnchorKey, (int) alignAnchor);
					alignTargetField.style.display = GetDisplay(value == ArrangeOnSceneMath.AlignAnchor.Target);
					SceneView.RepaintAll();
				})));
			alignBox.Add(alignTargetField);

			distributeAxes = (ArrangeOnSceneMath.Axes) SessionState.GetInt(DistributeAxesKey, (int) ArrangeOnSceneMath.Axes.None);

			distributeBox = new VisualElement();
			distributeBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			distributeBox.Add(CreateSectionTitle("Distribute"));
			distributeBox.Add(CreateDescription("Spreads the selection evenly between its current min and max on the chosen axes, ordered by its current spread on the other axes (e.g. left-to-right on X before distributing upward on Y)."));

			distributeBox.Add(CreateLabeledRow("Distribute Axes", BuildToggleButtonRow(
				AxesOptions,
				flag => (distributeAxes & flag) != 0,
				flag =>
				{
					distributeAxes ^= flag;
					SessionState.SetInt(DistributeAxesKey, (int) distributeAxes);
					SceneView.RepaintAll();
				})));

			stackAxes = (ArrangeOnSceneMath.Axes) SessionState.GetInt(StackAxesKey, (int) ArrangeOnSceneMath.Axes.None);
			stackSpacing = SessionState.GetFloat(StackSpacingKey, DefaultStackSpacing);

			stackBox = new VisualElement();
			stackBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			stackBox.Add(CreateSectionTitle("Stack"));
			stackBox.Add(CreateDescription("Lines the selection up with a fixed step, ordered by its current spread on the other axes (e.g. left-to-right on X before stacking upward on Y) - instead of spreading it across the current range like Distribute does."));

			stackBox.Add(CreateLabeledRow("Stack Axes", BuildToggleButtonRow(
				AxesOptions,
				flag => (stackAxes & flag) != 0,
				flag =>
				{
					stackAxes ^= flag;
					SessionState.SetInt(StackAxesKey, (int) stackAxes);
					SceneView.RepaintAll();
				})));

			var stackSpacingField = new FloatField("Spacing") { value = stackSpacing };
			stackSpacingField.RegisterValueChangedCallback(ev =>
			{
				stackSpacing = ev.newValue;
				SessionState.SetFloat(StackSpacingKey, stackSpacing);
				SceneView.RepaintAll();
			});
			stackBox.Add(stackSpacingField);

			circleRadiusMode = (ArrangeOnSceneMath.CircleRadiusMode) SessionState.GetInt(CircleRadiusModeKey, (int) ArrangeOnSceneMath.CircleRadiusMode.Auto);
			circleRadius = Mathf.Max(MinCircleRadius, SessionState.GetFloat(CircleRadiusKey, DefaultCircleRadius));
			circleCenterMode = (ArrangeOnSceneMath.CircleCenterMode) SessionState.GetInt(CircleCenterModeKey, (int) ArrangeOnSceneMath.CircleCenterMode.Auto);
			circleAngleOffset = Mathf.Clamp(SessionState.GetFloat(CircleAngleOffsetKey, DefaultCircleAngleOffset), 0f, MaxCircleAngleOffset);
			circleRotationMode = (ArrangeOnSceneMath.CircleRotationMode) SessionState.GetInt(CircleRotationModeKey, (int) ArrangeOnSceneMath.CircleRotationMode.None);

			circleBox = new VisualElement();
			circleBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			circleBox.Add(CreateSectionTitle("Circle"));
			circleBox.Add(CreateDescription("Spreads the selection evenly around a circle on the XZ plane, centered on the current selection or on a chosen target object. Each object takes whichever spot on the circle is closest to its current position."));

			var circleCenterTargetField = new ObjectField("Center Target") { objectType = typeof(Transform), allowSceneObjects = true };
			circleCenterTargetField.style.display = GetDisplay(circleCenterMode == ArrangeOnSceneMath.CircleCenterMode.Target);
			circleCenterTargetField.RegisterValueChangedCallback(ev =>
			{
				circleCenterTarget = ev.newValue as Transform;
				SaveTransform(CircleCenterTargetGlobalIdKey, circleCenterTarget);
				SceneView.RepaintAll();
			});
			LoadPersistedTransform(CircleCenterTargetGlobalIdKey, circleCenterTargetField, t => circleCenterTarget = t);

			circleBox.Add(CreateLabeledRow("Circle Center", BuildToggleButtonRow(
				new (ArrangeOnSceneMath.CircleCenterMode value, string label)[] { (ArrangeOnSceneMath.CircleCenterMode.Auto, "Auto"), (ArrangeOnSceneMath.CircleCenterMode.Target, "Target") },
				value => circleCenterMode == value,
				value =>
				{
					circleCenterMode = value;
					SessionState.SetInt(CircleCenterModeKey, (int) circleCenterMode);
					circleCenterTargetField.style.display = GetDisplay(value == ArrangeOnSceneMath.CircleCenterMode.Target);
					SceneView.RepaintAll();
				})));
			circleBox.Add(circleCenterTargetField);

			var circleRadiusField = new FloatField("Radius") { value = circleRadius };
			circleRadiusField.style.display = GetDisplay(circleRadiusMode == ArrangeOnSceneMath.CircleRadiusMode.Custom);
			circleRadiusField.RegisterValueChangedCallback(ev =>
			{
				circleRadius = Mathf.Max(MinCircleRadius, ev.newValue);
				circleRadiusField.SetValueWithoutNotify(circleRadius);
				SessionState.SetFloat(CircleRadiusKey, circleRadius);
				SceneView.RepaintAll();
			});

			circleBox.Add(CreateLabeledRow("Circle Radius", BuildToggleButtonRow(
				new (ArrangeOnSceneMath.CircleRadiusMode value, string label)[] { (ArrangeOnSceneMath.CircleRadiusMode.Auto, "Auto"), (ArrangeOnSceneMath.CircleRadiusMode.Custom, "Custom") },
				value => circleRadiusMode == value,
				value =>
				{
					circleRadiusMode = value;
					SessionState.SetInt(CircleRadiusModeKey, (int) circleRadiusMode);
					circleRadiusField.style.display = GetDisplay(value == ArrangeOnSceneMath.CircleRadiusMode.Custom);
					SceneView.RepaintAll();
				})));
			circleBox.Add(circleRadiusField);

			var circleAngleOffsetField = new Slider("Angle Offset", 0f, MaxCircleAngleOffset) { value = circleAngleOffset, showInputField = true };
			circleAngleOffsetField.RegisterValueChangedCallback(ev =>
			{
				circleAngleOffset = ev.newValue;
				SessionState.SetFloat(CircleAngleOffsetKey, circleAngleOffset);
				SceneView.RepaintAll();
			});
			circleBox.Add(circleAngleOffsetField);

			var circleRotationModeField = new DropdownField("Rotate Objects",
				new List<string> { "Don't Change", "Look At Center", "Match Target Rotation", "Look At Next In Circle" }, (int) circleRotationMode);
			circleRotationModeField.RegisterValueChangedCallback(_ =>
			{
				circleRotationMode = (ArrangeOnSceneMath.CircleRotationMode) circleRotationModeField.index;
				SessionState.SetInt(CircleRotationModeKey, (int) circleRotationMode);
				SceneView.RepaintAll();
			});
			circleBox.Add(circleRotationModeField);

			raycastDirection = (ArrangeOnSceneMath.RaycastDirection) SessionState.GetInt(RaycastDirectionKey, (int) ArrangeOnSceneMath.RaycastDirection.Down);
			raycastAlignWithNormal = SessionState.GetBool(RaycastAlignWithNormalKey, false);
			raycastLayerMask = SessionState.GetInt(RaycastLayerMaskKey, ~0);

			raycastBox = new VisualElement();
			raycastBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			raycastBox.Add(CreateSectionTitle("Raycast"));
			raycastBox.Add(CreateDescription("Casts a ray from each selected object's own position in the chosen direction and moves it to the hit point. Objects that don't hit anything on the chosen layers are left where they are."));

			var raycastDirectionTargetField = new ObjectField("Direction Target") { objectType = typeof(Transform), allowSceneObjects = true };
			raycastDirectionTargetField.style.display = GetDisplay(raycastDirection == ArrangeOnSceneMath.RaycastDirection.DirectionToTarget);
			raycastDirectionTargetField.RegisterValueChangedCallback(ev =>
			{
				raycastDirectionTarget = ev.newValue as Transform;
				SaveTransform(RaycastDirectionTargetGlobalIdKey, raycastDirectionTarget);
				SceneView.RepaintAll();
			});
			LoadPersistedTransform(RaycastDirectionTargetGlobalIdKey, raycastDirectionTargetField, t => raycastDirectionTarget = t);

			var raycastDirectionField = new DropdownField("Direction",
				new List<string> { "Left", "Right", "Up", "Down", "Forward", "Backward", "Direction To Target" }, (int) raycastDirection);
			raycastDirectionField.RegisterValueChangedCallback(_ =>
			{
				raycastDirection = (ArrangeOnSceneMath.RaycastDirection) raycastDirectionField.index;
				SessionState.SetInt(RaycastDirectionKey, (int) raycastDirection);
				raycastDirectionTargetField.style.display = GetDisplay(raycastDirection == ArrangeOnSceneMath.RaycastDirection.DirectionToTarget);
				SceneView.RepaintAll();
			});
			raycastBox.Add(raycastDirectionField);
			raycastBox.Add(raycastDirectionTargetField);

			var raycastAlignWithNormalToggle = new Toggle("Align With Normal") { value = raycastAlignWithNormal };
			raycastAlignWithNormalToggle.RegisterValueChangedCallback(ev =>
			{
				raycastAlignWithNormal = ev.newValue;
				SessionState.SetBool(RaycastAlignWithNormalKey, raycastAlignWithNormal);
				SceneView.RepaintAll();
			});
			raycastBox.Add(raycastAlignWithNormalToggle);

			// Same LayerMaskField used by Object Placer's own raycast layer filter, see ObjectPlacerGeneralSection.
			var raycastLayerMaskField = new LayerMaskField("Layers", raycastLayerMask);
			raycastLayerMaskField.RegisterValueChangedCallback(ev =>
			{
				raycastLayerMask = ev.newValue;
				SessionState.SetInt(RaycastLayerMaskKey, raycastLayerMask);
				SceneView.RepaintAll();
			});
			raycastBox.Add(raycastLayerMaskField);

			applyBtn = new Button(OnApplyClicked);
			applyBtn.AddToClassList(ApplyButtonClass);

			mode = ArrangeOnSceneToolState.Mode;

			var modeBox = new VisualElement();
			modeBox.AddToClassList(LevelDesignToolStyles.GroupBoxClass);
			modeBox.Add(CreateSectionTitle("Mode"));

			modeField = new DropdownField("", new List<string> { "Align", "Distribute", "Circle", "Stack", "Raycast" }, (int) mode);
			// Only sets the shared state - OnToolStateModeChanged (subscribed in OnEnable) is what actually applies
			// the new mode here, so this window and the Scene view overlay's own Mode dropdown stay in sync however
			// either one is changed.
			modeField.RegisterValueChangedCallback(_ => ArrangeOnSceneToolState.SetMode((ArrangeOnSceneMode) modeField.index));
			modeBox.Add(modeField);

			ApplyModeVisibility();

			var scrollView = new ScrollView(ScrollViewMode.Vertical);
			scrollView.Add(infoBox);
			scrollView.Add(modeBox);
			scrollView.Add(alignBox);
			scrollView.Add(distributeBox);
			scrollView.Add(stackBox);
			scrollView.Add(circleBox);
			scrollView.Add(raycastBox);
			scrollView.Add(applyBtn);

			root.Add(scrollView);
		}

		// Only the box for the currently selected Mode is shown - its settings and description double as "what
		// will happen" preview info, and OnSceneGUI always previews that same mode (see below), so picking a
		// mode is enough to see both its block and its Scene view preview without a separate preview toggle.
		// The single Apply button below replaces what used to be one button per block, its label switching to
		// match whichever mode is currently active. Also called from OnToolStateModeChanged, so guard against
		// running before CreateGUI has built these boxes yet.
		void ApplyModeVisibility()
		{
			if (alignBox == null)
				return;

			alignBox.style.display = GetDisplay(mode == ArrangeOnSceneMode.Align);
			distributeBox.style.display = GetDisplay(mode == ArrangeOnSceneMode.Distribute);
			circleBox.style.display = GetDisplay(mode == ArrangeOnSceneMode.Circle);
			stackBox.style.display = GetDisplay(mode == ArrangeOnSceneMode.Stack);
			raycastBox.style.display = GetDisplay(mode == ArrangeOnSceneMode.Raycast);
			applyBtn.text = GetApplyButtonText(mode);
		}

		// Same GlobalObjectId-based persistence as ObjectPlacerGeneralSection's Auto-Parent field - a scene object
		// has no asset GUID, so plain AssetDatabase GUIDs don't apply here. Shared by every "pick a scene Transform"
		// field in this window (Align's Target anchor, Circle's Target center).
		static void LoadPersistedTransform(string key, ObjectField field, Action<Transform> apply)
		{
			var idString = SessionState.GetString(key, string.Empty);
			if (string.IsNullOrEmpty(idString) || !GlobalObjectId.TryParse(idString, out var globalId))
				return;

			if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) is not Transform target)
				return;

			apply(target);
			field.SetValueWithoutNotify(target);
		}

		static void SaveTransform(string key, Transform target)
		{
			if (!target)
			{
				SessionState.EraseString(key);
				return;
			}

			var globalId = GlobalObjectId.GetGlobalObjectIdSlow(target.gameObject);
			SessionState.SetString(key, globalId.ToString());
		}

		static string GetApplyButtonText(ArrangeOnSceneMode value) => value switch
		{
			ArrangeOnSceneMode.Align => "Align",
			ArrangeOnSceneMode.Distribute => "Distribute Evenly",
			ArrangeOnSceneMode.Circle => "Arrange in Circle",
			ArrangeOnSceneMode.Stack => "Stack With Fixed Spacing",
			ArrangeOnSceneMode.Raycast => "Raycast To Ground",
			_ => "Apply",
		};

		void OnApplyClicked()
		{
			switch (mode)
			{
				case ArrangeOnSceneMode.Align:
					OnAlignClicked();
					break;
				case ArrangeOnSceneMode.Distribute:
					OnDistributeClicked();
					break;
				case ArrangeOnSceneMode.Circle:
					OnArrangeCircleClicked();
					break;
				case ArrangeOnSceneMode.Stack:
					OnStackClicked();
					break;
				case ArrangeOnSceneMode.Raycast:
					OnRaycastClicked();
					break;
			}
		}

		static (ArrangeOnSceneMath.Axes value, string label)[] AxesOptions => new[]
		{
			(ArrangeOnSceneMath.Axes.X, "X"), (ArrangeOnSceneMath.Axes.Y, "Y"), (ArrangeOnSceneMath.Axes.Z, "Z"),
		};

		// Builds a row of plain Buttons that behave like toggles (single or multiple active at once, depending on
		// what isActive/onClick do) - UI Toolkit's built-in ToggleButtonGroup only exists from Unity 6 onward, and
		// this repo targets 2022+, so the "active" look is just a USS class flipped on click instead.
		static VisualElement BuildToggleButtonRow<T>((T value, string label)[] options, Func<T, bool> isActive, Action<T> onClick)
		{
			var row = new VisualElement();
			row.AddToClassList(ToggleButtonRowClass);

			var buttons = new List<(T value, Button button)>();

			foreach (var (value, label) in options)
			{
				var button = new Button(() =>
				{
					onClick(value);
					RefreshToggleButtonRow(buttons, isActive);
				})
				{
					text = label,
				};
				button.AddToClassList(ToggleButtonClass);

				buttons.Add((value, button));
				row.Add(button);
			}

			// Seamless "segmented control" look: no gaps between buttons, square edges in the middle, rounded
			// only on the two outer corners - USS has no :first-child/:last-child, so these are explicit markers.
			buttons[0].button.AddToClassList(ToggleButtonFirstClass);
			buttons[^1].button.AddToClassList(ToggleButtonLastClass);

			RefreshToggleButtonRow(buttons, isActive);

			return row;
		}

		static StyleEnum<DisplayStyle> GetDisplay(bool isEnabled) => new (isEnabled ? StyleKeyword.Auto : StyleKeyword.None);

		// Matches the bold block-title convention from the Object Placer sections ("General", "Brush", "Palette").
		static Label CreateSectionTitle(string text) => new (text) { style = { unityFontStyleAndWeight = FontStyle.Bold } };

		static Label CreateDescription(string text) => new (text) { style = { whiteSpace = WhiteSpace.Normal } };

		// Puts the label to the left of the field instead of stacking it above, matching the layout every built-in
		// field (EnumFlagsField, FloatField, DropdownField, ...) already uses. Reuses Unity's own default USS
		// classes for the label/input split - those are always available in an EditorWindow, no custom style needed -
		// so the label width lines up with every other field in the same window automatically.
		static VisualElement CreateLabeledRow(string labelText, VisualElement field)
		{
			var row = new VisualElement();
			row.AddToClassList("unity-base-field");

			var label = new Label(labelText);
			label.AddToClassList("unity-base-field__label");
			row.Add(label);

			field.AddToClassList("unity-base-field__input");
			row.Add(field);

			return row;
		}

		static void RefreshToggleButtonRow<T>(List<(T value, Button button)> buttons, Func<T, bool> isActive)
		{
			foreach (var (value, button) in buttons)
				button.EnableInClassList(ToggleButtonActiveClass, isActive(value));
		}

		// Not gated on the window being focused, so it stays visible while the user tweaks the fields below.
		void OnSceneGUI(SceneView sceneView)
		{
			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			switch (mode)
			{
				case ArrangeOnSceneMode.Align:
					DrawAlignPreview(transforms);
					break;
				case ArrangeOnSceneMode.Distribute:
					DrawDistributePreview(transforms);
					break;
				case ArrangeOnSceneMode.Circle:
					DrawCirclePreview(transforms);
					break;
				case ArrangeOnSceneMode.Stack:
					DrawStackPreview(transforms);
					break;
				case ArrangeOnSceneMode.Raycast:
					DrawRaycastPreview(transforms);
					break;
			}
		}

		void DrawAlignPreview(Transform[] transforms)
		{
			if (alignAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var hasAxis = new[] { (alignAxes & ArrangeOnSceneMath.Axes.X) != 0, (alignAxes & ArrangeOnSceneMath.Axes.Y) != 0, (alignAxes & ArrangeOnSceneMath.Axes.Z) != 0 };
			var targets = new float[3];

			for (var axisIndex = 0; axisIndex < 3; axisIndex++)
				if (hasAxis[axisIndex])
					targets[axisIndex] = ArrangeOnSceneMath.GetAlignTarget(transforms, alignAnchor, alignTarget, axisIndex);

			foreach (var t in transforms)
			{
				var previewPosition = t.position;

				for (var axisIndex = 0; axisIndex < 3; axisIndex++)
					if (hasAxis[axisIndex])
						previewPosition = ArrangeOnSceneMath.SetAxisValue(previewPosition, axisIndex, targets[axisIndex]);

				ArrangePreviewDrawer.DrawPreviewPoint(t.position, previewPosition);
			}
		}

		void DrawDistributePreview(Transform[] transforms)
		{
			if (distributeAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var positions = ArrangeOnSceneMath.ComputeDistributedPositions(transforms, distributeAxes);

			for (var i = 0; i < transforms.Length; i++)
				ArrangePreviewDrawer.DrawPreviewPoint(transforms[i].position, positions[i]);

			// The shared arrange order's first/last objects are the two endpoints that anchor the spread - the same
			// pair regardless of which axes are checked, so just one marker pair instead of one per axis.
			var order = ArrangeOnSceneMath.GetArrangeOrder(transforms, distributeAxes);
			ArrangePreviewDrawer.DrawEndpointMarker(transforms[order[0]].position, ArrangePreviewDrawer.StartMarkerColor);
			ArrangePreviewDrawer.DrawEndpointMarker(transforms[order[^1]].position, ArrangePreviewDrawer.EndMarkerColor);
		}

		void DrawCirclePreview(Transform[] transforms)
		{
			var positions = ArrangeOnSceneMath.ComputeCirclePositions(transforms, circleRadiusMode, circleRadius, circleCenterMode, circleCenterTarget, circleAngleOffset);

			for (var i = 0; i < transforms.Length; i++)
				ArrangePreviewDrawer.DrawPreviewPoint(transforms[i].position, positions[i]);
		}

		void DrawStackPreview(Transform[] transforms)
		{
			if (stackAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var positions = ArrangeOnSceneMath.ComputeStackedPositions(transforms, stackAxes, stackSpacing);

			for (var i = 0; i < transforms.Length; i++)
				ArrangePreviewDrawer.DrawPreviewPoint(transforms[i].position, positions[i]);

			// The arrange order's first object is the anchor for every axis - the same one regardless of which axes
			// are checked, so just one marker instead of one per axis.
			var order = ArrangeOnSceneMath.GetArrangeOrder(transforms, stackAxes);
			ArrangePreviewDrawer.DrawEndpointMarker(transforms[order[0]].position, ArrangePreviewDrawer.StartMarkerColor);
		}

		void DrawRaycastPreview(Transform[] transforms)
		{
			foreach (var t in transforms)
				if (ArrangeOnSceneMath.TryRaycastPlacement(t, raycastDirection, raycastDirectionTarget, raycastLayerMask, RaycastMaxDistance, out var position, out _))
					ArrangePreviewDrawer.DrawPreviewPoint(t.position, position);
		}

		void OnAlignClicked()
		{
			if (alignAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			Undo.RecordObjects(transforms, AlignUndoName);

			if ((alignAxes & ArrangeOnSceneMath.Axes.X) != 0) ArrangeOnSceneMath.AlignAxis(transforms, alignAnchor, alignTarget, 0);
			if ((alignAxes & ArrangeOnSceneMath.Axes.Y) != 0) ArrangeOnSceneMath.AlignAxis(transforms, alignAnchor, alignTarget, 1);
			if ((alignAxes & ArrangeOnSceneMath.Axes.Z) != 0) ArrangeOnSceneMath.AlignAxis(transforms, alignAnchor, alignTarget, 2);
		}

		void OnDistributeClicked()
		{
			if (distributeAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			Undo.RecordObjects(transforms, DistributeUndoName);

			var positions = ArrangeOnSceneMath.ComputeDistributedPositions(transforms, distributeAxes);

			for (var i = 0; i < transforms.Length; i++)
				transforms[i].position = positions[i];
		}

		void OnArrangeCircleClicked()
		{
			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			Undo.RecordObjects(transforms, CircleUndoName);

			var positions = ArrangeOnSceneMath.ComputeCirclePositions(transforms, circleRadiusMode, circleRadius, circleCenterMode, circleCenterTarget, circleAngleOffset);

			for (var i = 0; i < transforms.Length; i++)
				transforms[i].position = positions[i];

			var center = ArrangeOnSceneMath.GetCircleCenter(transforms, circleCenterMode, circleCenterTarget);
			ArrangeOnSceneMath.ApplyCircleRotations(transforms, positions, center, circleRotationMode, circleCenterTarget);
		}

		void OnStackClicked()
		{
			if (stackAxes == ArrangeOnSceneMath.Axes.None)
				return;

			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			Undo.RecordObjects(transforms, StackUndoName);

			var positions = ArrangeOnSceneMath.ComputeStackedPositions(transforms, stackAxes, stackSpacing);

			for (var i = 0; i < transforms.Length; i++)
				transforms[i].position = positions[i];
		}

		void OnRaycastClicked()
		{
			var transforms = Selection.transforms;
			if (transforms.Length < MinObjectsToArrange)
				return;

			Undo.RecordObjects(transforms, RaycastUndoName);

			ArrangeOnSceneMath.ApplyRaycastPlacement(transforms, raycastDirection, raycastDirectionTarget, raycastAlignWithNormal, raycastLayerMask, RaycastMaxDistance);
		}
	}
}
