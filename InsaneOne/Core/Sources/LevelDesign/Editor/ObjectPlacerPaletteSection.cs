using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Palette block of the Object Placer tool: palette asset field, prefab icon grid and selection status.
	/// The assigned palette and selected entry are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerPaletteSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string PaletteGuidKey = "InsaneOne.ObjectPlacer.PaletteGuid";
		const string SelectedEntryIndexKey = "InsaneOne.ObjectPlacer.SelectedEntryIndex";

		public ObjectPalette.Entry SelectedEntry => palette && selectedEntryIndex >= 0 && selectedEntryIndex < palette.Entries.Count
			? palette.Entries[selectedEntryIndex]
			: null;

		readonly ObjectField paletteField;
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

			paletteGrid = new VisualElement();
			paletteGrid.AddToClassList("palette-grid");
			Add(paletteGrid);

			statusLabel = new Label { style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) } };
			Add(statusLabel);

			LoadPersistedSelection();

			RebuildPaletteGrid();
			UpdateStatusLabel();
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
			else if (selectedEntryIndex < 0 || selectedEntryIndex >= palette.Entries.Count)
				statusLabel.text = "Select a prefab from the palette.";
			else
				statusLabel.text = $"Selected: {palette.Entries[selectedEntryIndex].Prefab.name}";
		}
	}
}
