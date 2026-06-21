using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Locales.Editor
{
	internal class LocalizedDrawerState
	{
		public string prevLocale;
		public Label helpLabel;
	}

	[CustomPropertyDrawer(typeof(LocalizedAttribute))]
	public class LocalizedAttributeDrawer : PropertyDrawer
	{
		static readonly StyleColor DefaultColor = Color.gray;
		static readonly StyleColor WarningColor = Color.yellow;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var root = new VisualElement();
			var field = new PropertyField(property);
			root.Add(field);

			if (property.propertyType is not SerializedPropertyType.String)
				return root;

			field.RegisterValueChangeCallback(evt => OnChanged(evt, root));

			var helpLabel = new Label { style = { paddingLeft = 4, fontSize = 10, color = DefaultColor } };
			root.Add(helpLabel);
			root.userData = new LocalizedDrawerState { helpLabel = helpLabel };

			OnChanged(new SerializedPropertyChangeEvent { changedProperty = property }, root); // for first update

			return root;
		}

		void OnChanged(SerializedPropertyChangeEvent evt, VisualElement root)
		{
			if (Application.isPlaying || root.userData is not LocalizedDrawerState state)
				return;

			var locale = evt.changedProperty.stringValue;
			var helpLabel = state.helpLabel;

			if (string.IsNullOrWhiteSpace(locale))
			{
				helpLabel.text = "";
				state.prevLocale = "";
				return;
			}

			if (locale == state.prevLocale)
				return;

			Localization.Initialize();
			Localization.SetLanguage(Localization.Language); // default language

			if (!Localization.TryGetText(locale, out var localizedText))
			{
				localizedText = "Localization not found!";
				helpLabel.style.color = WarningColor;
			}
			else
			{
				helpLabel.style.color = DefaultColor;
			}

			helpLabel.text = localizedText;

			Localization.Unload();
			state.prevLocale = locale;
		}
	}
}