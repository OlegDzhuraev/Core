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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> General settings block of the Object Placer tool: tool activation, raycast layer filter, auto-parent and drag-scatter placement.
	/// Values are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerGeneralSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string LayerMaskKey = "InsaneOne.ObjectPlacer.LayerMask";
		const string ParentGlobalIdKey = "InsaneOne.ObjectPlacer.ParentGlobalId";
		const string DragScatterKey = "InsaneOne.ObjectPlacer.DragScatter";
		const string DragScatterSpacingKey = "InsaneOne.ObjectPlacer.DragScatterSpacing";

		public bool IsToolActive => activeToggle.value;
		public int LayerMask => layerMaskField.value;
		public Transform ParentTransform => parentField.value as Transform;
		public bool DragScatter => dragScatterToggle.value;
		public float DragScatterSpacing => dragScatterSpacingField.value;

		readonly Toggle activeToggle;
		readonly LayerMaskField layerMaskField;
		readonly ObjectField parentField;
		readonly Toggle dragScatterToggle;
		readonly Slider dragScatterSpacingField;

		public ObjectPlacerGeneralSection()
		{
			AddToClassList(GroupStyleName);

			Add(new Label("General") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			activeToggle = new Toggle("Placement Tool Active") { value = ObjectPlacerToolState.IsActive };
			activeToggle.RegisterValueChangedCallback(ev => ObjectPlacerToolState.SetActive(ev.newValue));

			layerMaskField = new LayerMaskField("Placement Layers", SessionState.GetInt(LayerMaskKey, ~0));
			layerMaskField.RegisterValueChangedCallback(ev => SessionState.SetInt(LayerMaskKey, ev.newValue));

			parentField = new ObjectField("Auto-Parent") { objectType = typeof(Transform), allowSceneObjects = true };
			parentField.RegisterValueChangedCallback(ev => SaveParent(ev.newValue as Transform));

			dragScatterToggle = new Toggle("Drag Scatter") { value = SessionState.GetBool(DragScatterKey, false) };
			dragScatterSpacingField = new Slider("Scatter Spacing", 0.1f, 10f) { value = SessionState.GetFloat(DragScatterSpacingKey, 1f), showInputField = true };
			dragScatterToggle.RegisterValueChangedCallback(ev =>
			{
				SessionState.SetBool(DragScatterKey, ev.newValue);
				dragScatterSpacingField.style.display = GetDisplay(ev.newValue);
			});
			dragScatterSpacingField.RegisterValueChangedCallback(ev => SessionState.SetFloat(DragScatterSpacingKey, ev.newValue));
			dragScatterSpacingField.style.display = GetDisplay(dragScatterToggle.value);

			Add(activeToggle);
			Add(layerMaskField);
			Add(parentField);
			Add(dragScatterToggle);
			Add(dragScatterSpacingField);

			LoadPersistedParent();

			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
			RegisterCallback<DetachFromPanelEvent>(_ => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged);
		}

		void LoadPersistedParent()
		{
			var idString = SessionState.GetString(ParentGlobalIdKey, string.Empty);
			if (string.IsNullOrEmpty(idString) || !GlobalObjectId.TryParse(idString, out var globalId))
				return;

			if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) is Transform transform)
				parentField.SetValueWithoutNotify(transform);
		}

		void SaveParent(Transform parent)
		{
			if (!parent)
			{
				SessionState.EraseString(ParentGlobalIdKey);
				return;
			}

			var globalId = GlobalObjectId.GetGlobalObjectIdSlow(parent.gameObject);
			SessionState.SetString(ParentGlobalIdKey, globalId.ToString());
		}

		void OnStateActiveChanged(bool value) => activeToggle.SetValueWithoutNotify(value);

		static StyleEnum<DisplayStyle> GetDisplay(bool isEnabled) => new (isEnabled ? StyleKeyword.Auto : StyleKeyword.None);
	}
}
