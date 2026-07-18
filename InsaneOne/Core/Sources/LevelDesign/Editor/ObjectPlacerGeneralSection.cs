using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> General settings block of the Object Placer tool: tool activation and raycast layer filter.
	/// Values are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerGeneralSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string LayerMaskKey = "InsaneOne.ObjectPlacer.LayerMask";

		public bool IsToolActive => activeToggle.value;
		public int LayerMask => layerMaskField.value;

		readonly Toggle activeToggle;
		readonly LayerMaskField layerMaskField;

		public ObjectPlacerGeneralSection()
		{
			AddToClassList(GroupStyleName);

			Add(new Label("General") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			activeToggle = new Toggle("Placement Tool Active") { value = ObjectPlacerToolState.IsActive };
			activeToggle.RegisterValueChangedCallback(ev => ObjectPlacerToolState.SetActive(ev.newValue));

			layerMaskField = new LayerMaskField("Placement Layers", SessionState.GetInt(LayerMaskKey, ~0));
			layerMaskField.RegisterValueChangedCallback(ev => SessionState.SetInt(LayerMaskKey, ev.newValue));

			Add(activeToggle);
			Add(layerMaskField);

			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
			RegisterCallback<DetachFromPanelEvent>(_ => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged);
		}

		void OnStateActiveChanged(bool value) => activeToggle.SetValueWithoutNotify(value);
	}
}
