using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Locales.Editor
{
	[CustomPropertyDrawer(typeof(LocalizedAttribute))]
	public class LocalizedAttributeDrawer : PropertyDrawer
	{
		string lastLocale;

		Label helpLabel;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var root = new VisualElement();
			var field = new PropertyField(property);
			root.Add(field);

			if (property.propertyType is not SerializedPropertyType.String)
				return root;

			field.RegisterValueChangeCallback(OnChanged);

			helpLabel = new Label { style = { fontSize = 10, color = Color.gray } };
			root.Add(helpLabel);

			OnChanged(new SerializedPropertyChangeEvent { changedProperty = property }); // for first update

			return root;
		}

		void OnChanged(SerializedPropertyChangeEvent evt)
		{
			var locale = evt.changedProperty.stringValue;
			if (locale == lastLocale)
				return;

			Localization.Initialize();
			Localization.SetLanguage(Localization.Language); // default language

			if (!Localization.TryGetText(locale, out var localizedText))
				localizedText = "Localization not found!";

			helpLabel.text = localizedText;

			Localization.Unload();
			lastLocale = locale;
		}
	}
}