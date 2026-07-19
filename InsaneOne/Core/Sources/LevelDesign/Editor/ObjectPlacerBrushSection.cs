using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Brush block of the Object Placer tool: brush asset field plus an inline editor for its placement settings.
	/// The assigned brush is persisted in SessionState, so it survives domain reloads within the same Editor session. When no
	/// asset is assigned, a temporary in-memory Brush is used instead, so placement always has settings to read and the user
	/// can still tweak them - those edits just aren't saved anywhere until an actual asset is assigned. </summary>
	public class ObjectPlacerBrushSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string BrushGuidKey = "InsaneOne.ObjectPlacer.BrushGuid";

		public Brush SelectedBrush => brush ? brush : GetOrCreateTemporaryBrush();

		readonly ObjectField brushField;
		readonly VisualElement brushInspectorContainer;
		readonly Label statusLabel;

		Brush brush;
		Brush temporaryBrush;

		public ObjectPlacerBrushSection()
		{
			AddToClassList(GroupStyleName);

			Add(new Label("Brush") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			brushField = new ObjectField("Brush Asset") { objectType = typeof(Brush), allowSceneObjects = false };
			brushField.RegisterValueChangedCallback(OnBrushChanged);
			Add(brushField);

			statusLabel = new Label { style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) } };
			Add(statusLabel);

			brushInspectorContainer = new VisualElement();
			Add(brushInspectorContainer);

			LoadPersistedBrush();
			RebuildBrushInspector();
		}

		Brush GetOrCreateTemporaryBrush()
		{
			if (!temporaryBrush)
				temporaryBrush = ScriptableObject.CreateInstance<Brush>();

			return temporaryBrush;
		}

		void LoadPersistedBrush()
		{
			var guid = SessionState.GetString(BrushGuidKey, string.Empty);
			if (string.IsNullOrEmpty(guid))
				return;

			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path))
				return;

			brush = AssetDatabase.LoadAssetAtPath<Brush>(path);
			if (brush)
				brushField.SetValueWithoutNotify(brush);
		}

		void OnBrushChanged(ChangeEvent<Object> ev)
		{
			brush = ev.newValue as Brush;
			SaveBrush();
			RebuildBrushInspector();
		}

		void SaveBrush()
		{
			if (!brush)
			{
				SessionState.EraseString(BrushGuidKey);
				return;
			}

			var path = AssetDatabase.GetAssetPath(brush);
			SessionState.SetString(BrushGuidKey, AssetDatabase.AssetPathToGUID(path));
		}

		void RebuildBrushInspector()
		{
			statusLabel.text = brush ? string.Empty : "No brush asset assigned - using a temporary, unsaved brush.";

			brushInspectorContainer.Clear();
			brushInspectorContainer.Add(new InspectorElement(SelectedBrush));
		}
	}
}
