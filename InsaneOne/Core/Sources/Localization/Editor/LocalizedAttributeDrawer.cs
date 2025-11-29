using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Locales.Editor
{
	[CustomPropertyDrawer(typeof(LocalizedAttribute))]
	public class LocalizedAttributeDrawer : PropertyDrawer
	{
		static readonly StyleColor DefaultColor = Color.gray;
		static readonly StyleColor WarningColor = Color.yellow;

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

			helpLabel = new Label { style = { paddingLeft = 4, fontSize = 10, color = DefaultColor } };
			root.Add(helpLabel);

			OnChanged(new SerializedPropertyChangeEvent { changedProperty = property }); // for first update

			return root;
		}

		void OnChanged(SerializedPropertyChangeEvent evt)
		{
			if (Application.isPlaying)
				return;

			var locale = evt.changedProperty.stringValue;
			if (locale == lastLocale)
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
			lastLocale = locale;
		}
	}
}