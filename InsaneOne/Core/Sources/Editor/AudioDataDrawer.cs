using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core
{
	[CustomPropertyDrawer(typeof(AudioData))]
	public class AudioDataDrawer : PropertyDrawer
	{
		const string StylesPath = "InsaneOne/ToolsStyles";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var style = Resources.Load(StylesPath) as StyleSheet;
			var box = new Foldout { text = property.displayName, tooltip = property.tooltip, value = true };
			var noClipsLabel = new Label("No clips are set!") { style = { color = Color.yellow } };

			box.styleSheets.Add(style);
			box.AddToClassList("audio-bg");

			box.Add(noClipsLabel);
			var (clipsProp, clipsField) = AddPropField(property, "clipVariations");
			clipsField.RegisterValueChangeCallback(evt => OnClipsFieldChanged(evt, noClipsLabel));
			AddPropField(property, "pitchRandom");
			AddPropField(property, "volume");
			AddPropField(property, "loop");

			UpdateNoClipsLabel(noClipsLabel, clipsProp);

			return box;

			(SerializedProperty, PropertyField) AddPropField(SerializedProperty parentProperty, string name)
			{
				var prop = parentProperty.FindPropertyRelative(name);
				var field = new PropertyField(prop);
				box.Add(field);
				return (prop, field);
			}
		}

		void OnClipsFieldChanged(SerializedPropertyChangeEvent spChangeEvt, Label noClipsLabel)
			=> UpdateNoClipsLabel(noClipsLabel, spChangeEvt.changedProperty);

		void UpdateNoClipsLabel(Label noClipsLabel, SerializedProperty clipsProperty)
			=> noClipsLabel.style.display = clipsProperty.arraySize == 0 ? DisplayStyle.Flex : DisplayStyle.None;
	}
}