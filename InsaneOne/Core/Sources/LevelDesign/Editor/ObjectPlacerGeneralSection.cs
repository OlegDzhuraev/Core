using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> General settings block of the Object Placer tool: tool activation, raycast layer filter and drag-scatter placement.
	/// Values are persisted in SessionState, so they survive domain reloads within the same Editor session. </summary>
	public class ObjectPlacerGeneralSection : VisualElement
	{
		const string GroupStyleName = "group-box";
		const string LayerMaskKey = "InsaneOne.ObjectPlacer.LayerMask";
		const string DragScatterKey = "InsaneOne.ObjectPlacer.DragScatter";
		const string DragScatterSpacingKey = "InsaneOne.ObjectPlacer.DragScatterSpacing";

		public bool IsToolActive => activeToggle.value;
		public int LayerMask => layerMaskField.value;
		public bool DragScatter => dragScatterToggle.value;
		public float DragScatterSpacing => dragScatterSpacingField.value;

		readonly Toggle activeToggle;
		readonly LayerMaskField layerMaskField;
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
			Add(dragScatterToggle);
			Add(dragScatterSpacingField);

			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
			RegisterCallback<DetachFromPanelEvent>(_ => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged);
		}

		void OnStateActiveChanged(bool value) => activeToggle.SetValueWithoutNotify(value);

		static StyleEnum<DisplayStyle> GetDisplay(bool isEnabled) => new (isEnabled ? StyleKeyword.Auto : StyleKeyword.None);
	}
}
