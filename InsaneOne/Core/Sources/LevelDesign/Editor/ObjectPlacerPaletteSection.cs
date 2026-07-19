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

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Palette block of the Object Placer tool: palette asset field, prefab icon grid and selection status.
	/// The assigned palette, selected entry and random-from-palette flag are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerPaletteSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string PaletteGuidKey = "InsaneOne.ObjectPlacer.PaletteGuid";
		const string SelectedEntryIndexKey = "InsaneOne.ObjectPlacer.SelectedEntryIndex";
		const string RandomFromPaletteKey = "InsaneOne.ObjectPlacer.RandomFromPalette";

		public ObjectPalette.Entry SelectedEntry => palette && selectedEntryIndex >= 0 && selectedEntryIndex < palette.Entries.Count
			? palette.Entries[selectedEntryIndex]
			: null;

		public bool CanPlace => palette && (randomFromPaletteToggle.value ? GetFirstAvailableEntryIndex() >= 0 : SelectedEntry != null);

		readonly ObjectField paletteField;
		readonly Toggle randomFromPaletteToggle;
		readonly VisualElement paletteGrid;
		readonly Label statusLabel;

		ObjectPalette palette;
		int selectedEntryIndex = -1;

		public ObjectPlacerPaletteSection()
		{
			AddToClassList(GroupStyleName);

			Add(new Label("Palette") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			paletteField = new ObjectField("Palette Asset") { objectType = typeof(ObjectPalette), allowSceneObjects = false };
			paletteField.RegisterValueChangedCallback(OnPaletteChanged);
			Add(paletteField);

			randomFromPaletteToggle = new Toggle("Random From Palette") { value = SessionState.GetBool(RandomFromPaletteKey, false) };
			randomFromPaletteToggle.RegisterValueChangedCallback(ev =>
			{
				SessionState.SetBool(RandomFromPaletteKey, ev.newValue);
				UpdateStatusLabel();
			});
			Add(randomFromPaletteToggle);

			paletteGrid = new VisualElement();
			paletteGrid.AddToClassList("palette-grid");
			Add(paletteGrid);

			statusLabel = new Label { style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) } };
			Add(statusLabel);

			LoadPersistedSelection();

			RebuildPaletteGrid();
			UpdateStatusLabel();
		}

		/// <summary> Returns the entry to place for a single click/drag step: a random placeable entry when Random From Palette is on, otherwise the selected entry. </summary>
		public ObjectPalette.Entry GetEntryToPlace()
		{
			if (!palette)
				return null;

			if (!randomFromPaletteToggle.value)
				return SelectedEntry;

			var placeableEntries = new List<ObjectPalette.Entry>();
			foreach (var entry in palette.Entries)
				if (entry.Prefab)
					placeableEntries.Add(entry);

			return placeableEntries.Count > 0 ? placeableEntries[Random.Range(0, placeableEntries.Count)] : null;
		}

		void LoadPersistedSelection()
		{
			var guid = SessionState.GetString(PaletteGuidKey, string.Empty);
			if (string.IsNullOrEmpty(guid))
				return;

			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path))
				return;

			palette = AssetDatabase.LoadAssetAtPath<ObjectPalette>(path);
			if (!palette)
				return;

			paletteField.SetValueWithoutNotify(palette);

			var savedIndex = SessionState.GetInt(SelectedEntryIndexKey, -1);
			selectedEntryIndex = savedIndex >= 0 && savedIndex < palette.Entries.Count && palette.Entries[savedIndex].Prefab
				? savedIndex
				: GetFirstAvailableEntryIndex();
		}

		void OnPaletteChanged(ChangeEvent<Object> ev)
		{
			palette = ev.newValue as ObjectPalette;
			SavePalette();

			selectedEntryIndex = GetFirstAvailableEntryIndex();
			SaveSelectedEntryIndex();

			RebuildPaletteGrid();
			UpdateStatusLabel();
		}

		void SavePalette()
		{
			if (!palette)
			{
				SessionState.EraseString(PaletteGuidKey);
				return;
			}

			var path = AssetDatabase.GetAssetPath(palette);
			SessionState.SetString(PaletteGuidKey, AssetDatabase.AssetPathToGUID(path));
		}

		void SaveSelectedEntryIndex() => SessionState.SetInt(SelectedEntryIndexKey, selectedEntryIndex);

		int GetFirstAvailableEntryIndex()
		{
			if (!palette)
				return -1;

			for (var i = 0; i < palette.Entries.Count; i++)
				if (palette.Entries[i].Prefab)
					return i;

			return -1;
		}

		void RebuildPaletteGrid()
		{
			paletteGrid.Clear();

			if (!palette || palette.Entries.Count == 0)
			{
				paletteGrid.Add(new Label("Assign a palette with prefabs to start placing."));
				return;
			}

			for (var i = 0; i < palette.Entries.Count; i++)
			{
				var entry = palette.Entries[i];
				if (!entry.Prefab)
					continue;

				var index = i;
				var button = new Button(() => SelectEntry(index)) { tooltip = entry.Prefab.name };
				button.AddToClassList("palette-item");
				if (index == selectedEntryIndex)
					button.AddToClassList("palette-item-selected");

				Texture icon = entry.Icon;
				if (!icon)
					icon = AssetPreview.GetAssetPreview(entry.Prefab);
				if (!icon)
					icon = AssetPreview.GetMiniThumbnail(entry.Prefab);

				var image = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
				image.style.width = Length.Percent(100);
				image.style.height = Length.Percent(100);
				button.Add(image);

				paletteGrid.Add(button);
			}

			if (IsAnyPreviewLoading())
				schedule.Execute(RebuildPaletteGrid).ExecuteLater(250);
		}

		bool IsAnyPreviewLoading()
		{
			if (!palette)
				return false;

			foreach (var entry in palette.Entries)
				if (entry.Prefab && !entry.Icon && AssetPreview.IsLoadingAssetPreview(entry.Prefab.GetInstanceID()))
					return true;

			return false;
		}

		void SelectEntry(int index)
		{
			selectedEntryIndex = index;
			SaveSelectedEntryIndex();
			RebuildPaletteGrid();
			UpdateStatusLabel();
		}

		void UpdateStatusLabel()
		{
			if (!palette)
				statusLabel.text = "No palette assigned.";
			else if (randomFromPaletteToggle.value)
				statusLabel.text = GetFirstAvailableEntryIndex() >= 0 ? "Placing a random prefab from the palette." : "Palette has no placeable prefabs.";
			else if (selectedEntryIndex < 0 || selectedEntryIndex >= palette.Entries.Count)
				statusLabel.text = "Select a prefab from the palette.";
			else
				statusLabel.text = $"Selected: {palette.Entries[selectedEntryIndex].Prefab.name}";
		}
	}
}
