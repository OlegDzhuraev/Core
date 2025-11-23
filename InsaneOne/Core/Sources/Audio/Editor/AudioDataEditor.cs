using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core
{
	[CustomPropertyDrawer(typeof(AudioData))]
	public class AudioDataEditor : PropertyDrawer
	{
		const string StylesPath = "InsaneOne/ToolsStyles";

		Foldout box;
		Label noClipsLabel;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var style = Resources.Load(StylesPath) as StyleSheet;
			box = new Foldout { text = property.displayName, tooltip = property.tooltip, value = true };
			noClipsLabel = new Label("No clips are set!") {style = {color = Color.yellow}};

			box.styleSheets.Add(style);
			box.AddToClassList("audio-bg");

			box.Add(noClipsLabel);
			var (clipsProp, clipsField) = AddPropField(property, "clipVariations");
			clipsField.RegisterValueChangeCallback(OnClipsFieldChanged);
			AddPropField(property, "pitchRandom");
			AddPropField(property, "volume");
			AddPropField(property, "loop");

			UpdateNoClipsLabel(clipsProp);

			return box;
		}

		void OnClipsFieldChanged(SerializedPropertyChangeEvent spChangeEvt) => UpdateNoClipsLabel(spChangeEvt.changedProperty);

		void UpdateNoClipsLabel(SerializedProperty clipsProperty)
		{
			noClipsLabel.style.display = clipsProperty.arraySize == 0 ? DisplayStyle.Flex : DisplayStyle.None;
		}

		public (SerializedProperty, PropertyField) AddPropField(SerializedProperty parentProperty, string name)
		{
			var prop = parentProperty.FindPropertyRelative(name);
			var field = new PropertyField(prop);
			box.Add(field);
			return (prop, field);
		}
	}
}